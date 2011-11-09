using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using TimetablePlanner;
using System.ComponentModel;

namespace TimetablePlannerUI
{
    public class ConfigController : DependencyObject
    {
        public BlockController BlockContext
        {
            get { return (BlockController)GetValue(BlockContextProperty); }
            set { SetValue(BlockContextProperty, value); }
        }

        public static readonly DependencyProperty BlockContextProperty =
            DependencyProperty.Register("BlockContext", typeof(BlockController), typeof(ConfigController), new UIPropertyMetadata(null));


        public ConfigController()
        {
            BlockContext = new BlockController();
        }
    }
}
