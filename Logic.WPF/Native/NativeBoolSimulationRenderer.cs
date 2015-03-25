// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using Logic.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Logic.Native
{
    public class NativeBoolSimulationRenderer : IBoolSimulationRenderer
    {
        public IRenderer Renderer { get; set; }
        public IStyle NullStateStyle { get; set; }
        public IStyle TrueStateStyle { get; set; }
        public IStyle FalseStateStyle { get; set; }
        public IList<IShape> Shapes { get; set; }
        public IDictionary<XBlock, BoolSimulation> Simulations { get; set; }

        private DrawingVisual[][] _dwCache = null;
        private BoolSimulation[] _cache = null;
        private bool _haveCache = false;

        private DrawingVisual RenderToCache(XBlock block, IStyle style)
        {
            var dw = new DrawingVisual();
            using (var dc = dw.RenderOpen())
            {
                block.Render(dc, Renderer, style);
                foreach (var pin in block.Pins)
                {
                    pin.Render(dc, Renderer, style);
                }
            }
            dw.Drawing.Freeze();
            return dw;
        }

        private void CreateCache()
        {
            XBlock[] blocks = Shapes
                .Where(s => s is XBlock)
                .Cast<XBlock>()
                .ToArray();

            _dwCache = new DrawingVisual[blocks.Length][];
            _cache = new BoolSimulation[blocks.Length];

            for (int i = 0; i < blocks.Length; i++)
            {
                XBlock block = blocks[i];
                _cache[i] = Simulations[block];
                _dwCache[i] = new DrawingVisual[3];
                _dwCache[i][0] = RenderToCache(block, TrueStateStyle);
                _dwCache[i][1] = RenderToCache(block, FalseStateStyle);
                _dwCache[i][2] = RenderToCache(block, NullStateStyle);
            }
        }

        private void RenderCache(object dc)
        {
            for (int i = 0; i < _dwCache.Length; i++)
            {
                switch (_cache[i].State)
                {
                    case true:
                        (dc as DrawingContext).DrawDrawing(_dwCache[i][0].Drawing);
                        break;
                    case false:
                        (dc as DrawingContext).DrawDrawing(_dwCache[i][1].Drawing);
                        break;
                    case null:
                    default:
                        (dc as DrawingContext).DrawDrawing(_dwCache[i][2].Drawing);
                        break;
                }
            }
        }

        public void Render(object dc)
        {
            if (!_haveCache)
            {
                CreateCache();
                _haveCache = true;
            }
            RenderCache(dc);
        }
    }
}
