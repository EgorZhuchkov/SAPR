using OxyPlot.Series;
using SAPR.ConstructionUtils;
using SAPR.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SAPR.UIElements
{
    /// <summary>
    /// Interaction logic for PlotWindow.xaml
    /// </summary>
    public partial class PlotWindow : Window
    {
        public PlotWindow(string plotMode, ProcessorViewModel processor, int rodIndex, Construction construction)
        {
            InitializeComponent();
            DiagramView.Model = new OxyPlot.PlotModel();
            Func<double, double> plotFunc;
            switch (plotMode)
            {
                case "U(x)":
                    plotFunc = value => processor.GetU(value, rodIndex);
                    break;
                case "N(x)":
                    plotFunc = value => processor.GetN(value, rodIndex);
                    break;
                case "Sigma(x)":
                    plotFunc = value => processor.GetSigma(value, rodIndex);
                    break;
                default:
                    throw new ArgumentException();
            }

            DiagramView.Model.Series.Add(new FunctionSeries(plotFunc, 0, construction.Rods[rodIndex].Length, 0.01));
            DiagramView.Model.Title = plotMode;

            DiagramView.Model.InvalidatePlot(true);
        }
    }
}
