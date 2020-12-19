using MathNet.Numerics.LinearAlgebra;
using Microsoft.Win32;
using SAPR.ConstructionUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SAPR.ViewModels
{
    public class ProcessorViewModel : INotifyPropertyChanged
    {
        public bool IsActive { set { OnPropertyChanged("ProcessorData"); } }
        private List<string> calculationReport = new List<string>();
        private List<Strain> concentratedStrains;
        private List<Strain> lengthwiseStrains;

        public string ProcessorData
        {
            get
            {
                if(cachedConstruction.IsProcessed)
                {
                    return String.Join("\r\n", calculationReport.ToArray()); ;
                }
                else if(cachedConstruction.IsValid())
                {
                    return "Данные не рассчитаны";
                }
                else
                {
                    return "Конструкция задана некорректно";
                }
            }
        }
        private Construction cachedConstruction;

        public ProcessorViewModel(Construction construction)
        {
            UpdateProcessor(construction);
        }

        public void UpdateProcessor(Construction construction)
        {
            cachedConstruction = construction;
            concentratedStrains = cachedConstruction.Strains.Where(strain => strain.StrainType == StrainType.Concentrated).ToList();
            concentratedStrains = ReshapeConcentratedStrains(concentratedStrains, cachedConstruction.Rods.Count);
            lengthwiseStrains = cachedConstruction.Strains.Where(strain => strain.StrainType == StrainType.Lengthwise).ToList();
            lengthwiseStrains = ReshapeLengthwiseStrains(lengthwiseStrains, cachedConstruction.Rods.Count);

            if (cachedConstruction.IsProcessed)
            {
                CalculateResults();
            }
        }

        #region Commands
        private RelayCommand _processConstructionCommand;
        public RelayCommand ProcessConstructionCommand
        {
            get
            {
                return _processConstructionCommand ??
                  (_processConstructionCommand = new RelayCommand(obj =>
                  {
                      CalculateResults();
                  },
                  (obj) => (!cachedConstruction.IsProcessed && cachedConstruction.IsValid())));
            }
        }

        private RelayCommand _saveResultsCommand;
        public RelayCommand SaveResultsCommand
        {
            get
            {
                return _saveResultsCommand ??
                  (_saveResultsCommand = new RelayCommand(obj =>
                  {
                      SaveReportToFile();
                  },
                  (obj) => cachedConstruction.IsProcessed));
            }
        }
        #endregion

        private void CalculateResults()
        {
            calculationReport.Clear();
            #region Reaction matrix
            var reactionMatrix = Matrix<double>.Build.Sparse(cachedConstruction.Rods.Count + 1, cachedConstruction.Rods.Count + 1, 0);
            concentratedStrains = cachedConstruction.Strains.Where(strain => strain.StrainType == StrainType.Concentrated).ToList();
            concentratedStrains = ReshapeConcentratedStrains(concentratedStrains, cachedConstruction.Rods.Count);
            lengthwiseStrains = cachedConstruction.Strains.Where(strain => strain.StrainType == StrainType.Lengthwise).ToList();
            lengthwiseStrains = ReshapeLengthwiseStrains(lengthwiseStrains, cachedConstruction.Rods.Count);

            var blocks = new List<Matrix<double>>();

            //Fill reaction matrix
            for (var i = 0; i < cachedConstruction.Rods.Count; ++i)
            {
                var block = Matrix<double>.Build.Sparse(2, 2, 0);
                for (var x = 0; x < 2; ++x)
                    for (var y = 0; y < 2; ++y)
                        block[x, y] = cachedConstruction.Rods[i].Elasticity * cachedConstruction.Rods[i].Area / cachedConstruction.Rods[i].Length;
                block[1, 0] *= -1;
                block[0, 1] *= -1;
                blocks.Add(block);
            }

            for (var k = 0; k < blocks.Count; ++k)
            {
                var block = blocks[k];
                for (var i = k; i < k + 2; ++i)
                {
                    for (var j = k; j < k + 2; ++j)
                    {
                        if (i == j &&
                            (i != 0 || i != cachedConstruction.Rods.Count))
                        {
                            reactionMatrix[i, j] += block[i - k, j - k];
                        }
                        else
                        {
                            reactionMatrix[i, j] = block[i - k, j - k];
                        }
                    }
                }
            }

            //Set zero values 
            if (cachedConstruction.HasLeftSupport)
            {
                for (var i = 1; i < cachedConstruction.Rods.Count + 1; ++i)
                {
                    reactionMatrix[0, i] = reactionMatrix[i, 0] = 0;
                }
            }
                
            if (cachedConstruction.HasRightSupport)
            {
                for (var i = cachedConstruction.Rods.Count - 1; i > 0; --i)
                {
                    reactionMatrix[cachedConstruction.Rods.Count, i] = reactionMatrix[i, cachedConstruction.Rods.Count] = 0;
                }
            }

            if (cachedConstruction.Rods.Count == 1)
            {
                reactionMatrix[0, 1] = reactionMatrix[1, 0] = 0;
            }

            calculationReport.Add($"Матрица реакций:\n{reactionMatrix.ToMatrixString()}");
            #endregion

            #region Reaction vector
            var reactionVector = Vector<double>.Build.Sparse(cachedConstruction.Rods.Count + 1, 0);

            for (var i = 0; i < cachedConstruction.Rods.Count + 1; ++i)
            { 
                if (i == 0 && cachedConstruction.HasLeftSupport)
                {
                    continue;
                }
                    
                if (i == cachedConstruction.Rods.Count && cachedConstruction.HasRightSupport)
                {
                    continue;
                }
                    

                if (i > 0 && i < cachedConstruction.Rods.Count)
                {
                    reactionVector[i] = concentratedStrains[i].Magnitude + lengthwiseStrains[i].Magnitude * 
                        cachedConstruction.Rods[i].Length / 2 + lengthwiseStrains[i - 1].Magnitude * cachedConstruction.Rods[i - 1].Length / 2;
                }

                if (i == 0)
                {
                    reactionVector[i] = concentratedStrains[i].Magnitude + lengthwiseStrains[i].Magnitude * cachedConstruction.Rods[i].Length / 2;
                }

                if (i == cachedConstruction.Rods.Count) 
                {
                    reactionVector[i] = concentratedStrains[i].Magnitude + lengthwiseStrains[i - 1].Magnitude * cachedConstruction.Rods[i - 1].Length / 2;
                }
            }

            calculationReport.Add($"Вектор реакций:\n{reactionVector.ToVectorString()}");
            #endregion

            #region Deltas
            var deltas = reactionMatrix.Solve(reactionVector);

            calculationReport.Add($"Дельты:\n{deltas.ToVectorString()}");
            #endregion

            #region Ux
            var ux = Matrix<double>.Build.Dense(cachedConstruction.Rods.Count, 2, 0);

            for (var i = 0; i < cachedConstruction.Rods.Count; ++i)
            {
                ux[i, 0] = deltas[i];
            }

            for (var i = 0; i < cachedConstruction.Rods.Count; ++i)
            {
                ux[i, 1] = deltas[i + 1];
            }

            if (cachedConstruction.HasLeftSupport)
            {
                ux[0, 0] = 0;
            }

            if (cachedConstruction.HasRightSupport)
            {
                ux[cachedConstruction.Rods.Count - 1, 1] = 0;
            }

            calculationReport.Add($"Ux:\n{ux.ToMatrixString()}");
            cachedConstruction.Ux = ux;
            #endregion

            #region Nx 
            var nx = Matrix<double>.Build.Dense(cachedConstruction.Rods.Count, 2, 0);

            for (var i = 0; i < cachedConstruction.Rods.Count; ++i)
            {
                
                nx[i, 0] = (cachedConstruction.Rods[i].Elasticity * cachedConstruction.Rods[i].Area / cachedConstruction.Rods[i].Length) *
                    (ux[i, 1] - ux[i, 0]) + (lengthwiseStrains[i].Magnitude * cachedConstruction.Rods[i].Length / 2);
                nx[i, 1] = (cachedConstruction.Rods[i].Elasticity * cachedConstruction.Rods[i].Area / cachedConstruction.Rods[i].Length) *
                    (ux[i, 1] - ux[i, 0]) + (lengthwiseStrains[i].Magnitude * cachedConstruction.Rods[i].Length / 2) * 
                    (1 - 2 * cachedConstruction.Rods[i].Length / cachedConstruction.Rods[i].Length);
            }

            calculationReport.Add($"Nx:\n{nx.ToMatrixString()}");
            #endregion

            cachedConstruction.IsProcessed = true;
            OnPropertyChanged("ProcessorData");
        }

        private static List<Strain> ReshapeLengthwiseStrains(List<Strain> lengthwiseStrains, int rodsAmount)
        {
            var result = new List<Strain>(Enumerable.Repeat(new Strain { StrainType = StrainType.Concentrated }, rodsAmount));

            for (var i = 0; i < rodsAmount; ++i)
            {
                foreach (var lengthwiseStrain in lengthwiseStrains.Where(lengthwiseStrain => lengthwiseStrain.NodeIndex - 1 == i)) 
                { 
                    result[i] = lengthwiseStrain; 
                }
            }
            
            return result;
        }

        private static List<Strain> ReshapeConcentratedStrains(List<Strain> concentratedStrains, int rodsAmount)
        {
            var result = new List<Strain>(Enumerable.Repeat(new Strain { StrainType = StrainType.Concentrated }, rodsAmount + 1));

            for (var i = 0; i < rodsAmount + 1; ++i)
            {
                foreach (var concentratedStrain in concentratedStrains.Where(concentratedStrain => concentratedStrain.NodeIndex - 1 == i)) 
                { 
                    result[i] = concentratedStrain; 
                }
            }

            return result;
        }

        public double GetN(double x, int rod)
        {
            return (cachedConstruction.Rods[rod].Elasticity * cachedConstruction.Rods[rod].Area / cachedConstruction.Rods[rod].Length) * 
                (cachedConstruction.Ux[rod, 1] - cachedConstruction.Ux[rod, 0]) + 
                (lengthwiseStrains[rod].Magnitude * cachedConstruction.Rods[rod].Length / 2) * 
                (1 - 2 * x / cachedConstruction.Rods[rod].Length);
        }

        public double GetU(double x, int rod)
        {
            return cachedConstruction.Ux[rod, 0] +
                   (x / cachedConstruction.Rods[rod].Length) * 
                   (cachedConstruction.Ux[rod, 1] - cachedConstruction.Ux[rod, 0]) +
                   (lengthwiseStrains[rod].Magnitude * Math.Pow(cachedConstruction.Rods[rod].Length, 2)) / 
                   (2 * cachedConstruction.Rods[rod].Elasticity * cachedConstruction.Rods[rod].Area) * 
                   (x / cachedConstruction.Rods[rod].Length) * 
                   (1 - x / cachedConstruction.Rods[rod].Length);
        }

        public double GetSigma(double x, int rod)
        {
            return GetN(x, rod) / cachedConstruction.Rods[rod].Length;
        }

        private void SaveReportToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, ProcessorData);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
