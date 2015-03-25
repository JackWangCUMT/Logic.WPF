// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Portable;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Native
{
    public class NativeFileDialog : IFileDialog
    {
        #region All

        public FileDialogResult GetAllFileNameToOpen()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "All Files (*.*)|*.*",
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        #endregion

        #region Graph

        public FileDialogResult GetGraphFileNameToSave(string defaultFileName)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Graph (*.txt)|*.txt",
                FileName = defaultFileName
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        #endregion

        #region Block

        public FileDialogResult GetBlockFileNameToOpen()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Logic Block (*.lblock)|*.lblock",
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        public FileDialogResult GetBlockFileNameToSave(string defaultFileName)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Logic Block (*.lblock)|*.lblock",
                FileName = defaultFileName
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        #endregion

        #region Template

        public FileDialogResult GetTemplateFileNameToOpen()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Logic Template (*.ltemplate)|*.ltemplate",
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        public FileDialogResult GetTemplateFileNameToSave(string defaultFileName)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Logic Template (*.ltemplate)|*.ltemplate",
                FileName = defaultFileName
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        #endregion

        #region Project

        public FileDialogResult GetProjetFileNameToOpen()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Logic Project (*.lproject)|*.lproject"
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        public FileDialogResult GetProjetFileNameToSave(string defaultFileName)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Logic Project (*.lproject)|*.lproject",
                FileName = defaultFileName
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        #endregion

        #region Pdf

        public FileDialogResult GetPdfFileNameToSave(string defaultFileName)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = defaultFileName
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        #endregion

        #region CSharp

        public FileDialogResult GetCSharpFileNameToOpen()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        public FileDialogResult GetCSharpFileNamesToOpen()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, dlg.FileNames);
            }

            return new FileDialogResult(false, null, null);
        }

        public FileDialogResult GetCSharpFileNameToSave(string defaultFileName)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                FileName = defaultFileName
            };

            if (dlg.ShowDialog() == true)
            {
                return new FileDialogResult(true, dlg.FileName, null);
            }

            return new FileDialogResult(false, null, null);
        }

        #endregion
    }
}
