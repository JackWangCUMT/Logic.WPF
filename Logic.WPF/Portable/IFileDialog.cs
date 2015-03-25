// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Portable
{
    public interface IFileDialog
    {
        FileDialogResult GetAllFileNameToOpen();
        FileDialogResult GetBlockFileNameToOpen();
        FileDialogResult GetBlockFileNameToSave(string defaultFileName);
        FileDialogResult GetCSharpFileNamesToOpen();
        FileDialogResult GetCSharpFileNameToOpen();
        FileDialogResult GetCSharpFileNameToSave(string defaultFileName);
        FileDialogResult GetGraphFileNameToSave(string defaultFileName);
        FileDialogResult GetPdfFileNameToSave(string defaultFileName);
        FileDialogResult GetProjetFileNameToOpen();
        FileDialogResult GetProjetFileNameToSave(string defaultFileName);
        FileDialogResult GetTemplateFileNameToOpen();
        FileDialogResult GetTemplateFileNameToSave(string defaultFileName);
    }
}
