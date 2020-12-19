using SAPR.ConstructionUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SAPR.UIElements
{
    /// <summary>
    /// Interaction logic for ConstructionCanvas.xaml
    /// </summary>
    public partial class ConstructionCanvas : UserControl
    {
        private ObservableCollection<Rod> _rods;
        private ObservableCollection<Strain> _strains;
        private SupportMode _supportMode;

        public ConstructionCanvas(ObservableCollection<Rod> rods, ObservableCollection<Strain> strains, SupportMode supportMode)
        {
            InitializeComponent();

            _rods = rods;
            _strains = strains;
            _supportMode = supportMode;
        }

        public void UpdateConstructionCanvas(ObservableCollection<Rod> rods, ObservableCollection<Strain> strains, SupportMode supportMode)
        {
            _rods = rods;
            _strains = strains;
            _supportMode = supportMode;
        }

        public void ClearConstruction()
        {
            ConstructionCanvasUI.Children.Clear();
        }

        public void ShowDebugInfo()
        {
            MessageBox.Show($"Support mode: {_supportMode}");
        }

        public void DrawConstruction(SupportMode supportMode)
        {
            _supportMode = supportMode;
            ConstructionCanvasUI.Children.Clear();

            var concentratedStrains = _strains.ToList().Where(strain => strain.StrainType == StrainType.Concentrated);
            var lengthwiseStrains = _strains.ToList().Where(strain => strain.StrainType == StrainType.Lengthwise);

            var height = this.ActualHeight;
            var width = this.ActualWidth;

            var heightModifier = height / 350.0f;
            var widthModifier = width / 550.0f;

            var heightOffset = 100.0f * heightModifier;
            var widthOffset = 50.0f * widthModifier;

            var rodsTotalLength = _rods.ToList().Select(rod => rod.Length).Sum();
            var rodsMaxArea = _rods.ToList().Select(rod => rod.Area).Max();

            var rodsMinWidth = _rods.ToList().Select(rod => rod.Length).Min();
            var rodsMinHeight = _rods.ToList().Select(rod => rod.Area).Min();

            var rectangleWidthDevisor = rodsTotalLength / (width - 2 * widthOffset);
            var rectangleHeightDevisor = rodsMaxArea / (height - 2 * heightOffset);

            double currentOffset = widthOffset;
            var numberOfArrows = 8;

            #region Supports

            var leftSupport = new Path()
            {
                Data = Geometry.Parse("M 0 20 L 10 10 M 0 30 L 10 20 M 0 40 L 10 30 M 0 50 L 10 40 M 0 60 L 10 50 M 0 70 L 10 60 M 0 80 L 10 70 M 0 90 L 10 80 M 10 10 L 10 80"),
                Stroke = Brushes.Black,
                RenderTransformOrigin = Point.Parse(".5,.5")
            };

            Canvas.SetLeft(leftSupport, currentOffset - 10);
            Canvas.SetTop(leftSupport, height / 2 - 45);

            var rightSupport = new Path()
            {
                Data = Geometry.Parse("M 0 10 L 10 0 M 0 20 L 10 10 M 0 30 L 10 20 M 0 40 L 10 30 M 0 50 L 10 40 M 0 60 L 10 50 M 0 70 L 10 60 M 0 80 L 10 70 M 10 0 L 10 70"),
                Stroke = Brushes.Black,
                RenderTransform = new RotateTransform(180),
                RenderTransformOrigin = Point.Parse(".5,.5")
            };

            Canvas.SetRight(rightSupport, currentOffset - 10);
            Canvas.SetTop(rightSupport, height / 2 - 45);

            if (_supportMode == SupportMode.Both)
            {
                ConstructionCanvasUI.Children.Add(leftSupport);
                ConstructionCanvasUI.Children.Add(rightSupport);
            }
            else if (_supportMode == SupportMode.Left)
            {
                ConstructionCanvasUI.Children.Add(leftSupport);
            }
            else
            {
                ConstructionCanvasUI.Children.Add(rightSupport);
            }

            #endregion

            for (int i = 0; i < _rods.Count; i++)
            {
                var rodWidth = Math.Clamp(_rods[i].Length / rectangleWidthDevisor, 10.0f, float.MaxValue);
                var rodHeight = Math.Clamp(_rods[i].Area / rectangleHeightDevisor, 10.0f, float.MaxValue);

                var rodRectangle = new Rectangle()
                {
                    Height = Math.Clamp(rodHeight, 10.0f, float.MaxValue),
                    Width = rodWidth,
                    Stroke = Brushes.Black,
                    Fill = Brushes.LightGray,
                    ToolTip = $"Длина = {_rods[i].Length}, площадь = {_rods[i].Area}, модуль упругости = {_rods[i].Elasticity}, допускаемое напряжение = {_rods[i].AllowedStress}"
                };

                Canvas.SetTop(rodRectangle, height / 2 - rodHeight / 2);
                Canvas.SetLeft(rodRectangle, currentOffset);

                ConstructionCanvasUI.Children.Add(rodRectangle);

                if(lengthwiseStrains.Count(strain => strain.NodeIndex - 1 == i) == 1)
                {
                    for (int j = 0; j < numberOfArrows; j++)
                    {
                        var strain = lengthwiseStrains.First(strain => strain.NodeIndex - 1 == i);
                        var strainArrow = new Path
                        {
                            Data = Geometry.Parse("M 0,115 60,115 35,90 100,120 35,150 60,125 0,125 Z"),
                            Height = Math.Clamp(rodHeight / 10, 5.0f, float.MaxValue),
                            Width = rodWidth / numberOfArrows,
                            RenderTransformOrigin = Point.Parse(".5,.5"),
                            RenderTransform = strain.Magnitude < 0 ? new RotateTransform(180) : new RotateTransform(0),
                            Stretch = Stretch.Fill,
                            Fill = strain.Magnitude < 0 ? Brushes.DarkBlue : Brushes.DarkRed,
                            ToolTip = strain.Magnitude
                        };

                        Canvas.SetLeft(strainArrow, currentOffset + rodWidth / numberOfArrows * j);
                        Canvas.SetTop(strainArrow, height / 2 - strainArrow.Height / 2);

                        ConstructionCanvasUI.Children.Add(strainArrow);
                    }
                }

                var concentratedStrainsInCurrentNode = concentratedStrains.Where(strain => strain.NodeIndex - 1 == i);

                foreach (var strain in concentratedStrainsInCurrentNode)
                {
                    if (i == 0 && strain.Magnitude < 0.0f && _supportMode != SupportMode.Right)
                    {
                        continue;
                    }

                    var strainArrow = new Path
                    {
                        Data = Geometry.Parse("M 0,115 60,115 35,90 100,120 35,150 60,125 0,125 Z"),
                        Height = Math.Clamp((rodsMinHeight / rectangleHeightDevisor) / 3, 5.0f, float.MaxValue),
                        Width = Math.Clamp((rodsMinWidth / rectangleWidthDevisor) / 3, 5.0f, widthOffset - 5.0f),
                        RenderTransformOrigin = Point.Parse("0,.5"),
                        RenderTransform = strain.Magnitude < 0 ? new RotateTransform(180) : new RotateTransform(0),
                        Stretch = Stretch.Fill,
                        Fill = strain.Magnitude < 0 ? Brushes.BlueViolet : Brushes.OrangeRed,
                        Stroke = strain.Magnitude < 0 ? Brushes.DarkViolet : Brushes.DarkOrange,
                        ToolTip = strain.Magnitude
                    };

                    Canvas.SetLeft(strainArrow, currentOffset);
                    Canvas.SetTop(strainArrow, height / 2 - strainArrow.Height / 2);

                    ConstructionCanvasUI.Children.Add(strainArrow);
                }

                currentOffset += rodWidth;
            }

            var concentratedStrainInLastNode = concentratedStrains.Where(strain => strain.NodeIndex == _rods.Count + 1);

            if(concentratedStrainInLastNode.Any())
            {
                foreach (var strain in concentratedStrainInLastNode)
                {
                    if (strain.Magnitude > 0.0f && _supportMode != SupportMode.Left)
                    {
                        continue;
                    }

                    var strainArrow = new Path
                    {
                        Data = Geometry.Parse("M 0,115 60,115 35,90 100,120 35,150 60,125 0,125 Z"),
                        Height = Math.Clamp((rodsMinHeight / rectangleHeightDevisor) / 3, 1.0f, float.MaxValue),
                        Width = Math.Clamp((rodsMinWidth / rectangleWidthDevisor) / 3, 1.0f, widthOffset - 5.0f),
                        RenderTransformOrigin = Point.Parse("0,.5"),
                        RenderTransform = strain.Magnitude < 0 ? new RotateTransform(180) : new RotateTransform(0),
                        Stretch = Stretch.Fill,
                        Fill = strain.Magnitude < 0 ? Brushes.BlueViolet : Brushes.OrangeRed,
                        Stroke = strain.Magnitude < 0 ? Brushes.DarkViolet : Brushes.DarkOrange,
                        ToolTip = strain.Magnitude
                    };

                    Canvas.SetLeft(strainArrow, currentOffset);
                    Canvas.SetTop(strainArrow, height / 2 - strainArrow.Height / 2);

                    ConstructionCanvasUI.Children.Add(strainArrow);
                }
            }
        }
    }
}
