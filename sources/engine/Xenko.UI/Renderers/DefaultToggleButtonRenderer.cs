// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

using Xenko.Core;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.UI.Controls;

namespace Xenko.UI.Renderers
{
    /// <summary>
    /// The default renderer for <see cref="ToggleButton"/>.
    /// </summary>
    internal class DefaultToggleButtonRenderer : ElementRenderer
    {
        public DefaultToggleButtonRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void RenderColor(UIElement element, UIRenderingContext context, UIBatch Batch)
        {
            base.RenderColor(element, context, Batch);

            var toggleButton = (ToggleButton)element;
            var sprite = GetToggleStateImage(toggleButton);
            if (sprite?.Texture == null)
                return;
            
            var color = toggleButton.RenderOpacity * Color.White;
            Batch.DrawImage(sprite.Texture, ref element.WorldMatrixInternal, ref sprite.RegionInternal, ref element.RenderSizeInternal, ref sprite.BordersInternal, ref color, context.DepthBias, sprite.Orientation);
        }

        private static Sprite GetToggleStateImage(ToggleButton toggleButton)
        {
            switch (toggleButton.State)
            {
                case ToggleState.Checked:
                    return toggleButton.CheckedImage?.GetSprite();
                case ToggleState.Indeterminate:
                    return toggleButton.IndeterminateImage?.GetSprite();
                case ToggleState.UnChecked:
                    return toggleButton.UncheckedImage?.GetSprite();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
