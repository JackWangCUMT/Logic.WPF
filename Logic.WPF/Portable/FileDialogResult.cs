// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Portable
{
    public class FileDialogResult
    {
        public bool Success { get; private set; }
        public string FileName { get; private set; }
        public string[] FileNames { get; private set; }

        public FileDialogResult(
            bool success,
            string fileName,
            string[] fileNames)
        {
            this.Success = success;
            this.FileName = fileName;
            this.FileNames = fileNames;
        }
    }
}
