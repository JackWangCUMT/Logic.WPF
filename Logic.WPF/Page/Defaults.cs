using Logic.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Page
{
    public static class Defaults
    {
        public static string PageName = "Page";
        public static string DocumentName = "Document";
        public static string ProjectName = "Project";

        public static IPage EmptyPage()
        {
            return new XPage()
            {
                Name = Defaults.PageName,
                Shapes = new ObservableCollection<IShape>(),
                Blocks = new ObservableCollection<IShape>(),
                Pins = new ObservableCollection<IShape>(),
                Wires = new ObservableCollection<IShape>(),
                Template = null
            };
        }

        public static IDocument EmptyDocument()
        {
            return new XDocument()
            {
                Name = Defaults.DocumentName,
                Pages = new ObservableCollection<IPage>()
            };
        }

        public static IProject EmptyProject()
        {
            return new XProject()
            {
                Name = Defaults.ProjectName,
                Styles = new ObservableCollection<IStyle>(),
                Templates = new ObservableCollection<ITemplate>(),
                Documents = new ObservableCollection<IDocument>()
            };
        }
    }
}
