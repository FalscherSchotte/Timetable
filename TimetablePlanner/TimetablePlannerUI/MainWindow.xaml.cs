using System;
using System.Collections.Generic;
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
using TimetablePlanner;

namespace TimetablePlannerUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //this.DataContext = GetGenerator();
        }

        //private object GetGenerator()
        //{
        //    string configurationFile = "E:\\HsKA\\Semester2\\Projektarbeit\\Stundenplangenerator\\TimetablePlanner\\TimetablePlanner\\TimetableData.xml";
        //    TimetableData data = TimetableDataReader.createTimetableInstance(configurationFile);

        //    int populationSize = 75;
        //    int numberOfGenerations = 2000;

        //    TimetableGenerator generator = new TimetableGenerator(populationSize, data);
        //    generator.PerformEvolution(numberOfGenerations);
        //    return generator;
        //}
    }
}
