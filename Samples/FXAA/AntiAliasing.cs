using System;
using tainicom.Aether.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Samples.FXAA
{
    public class AntiAliasing
    {
        GraphicsDevice _graphicsDevice;
        SpriteBatch _spriteBatch;
        RenderTarget2D _renderTarget;

        internal FXAAEffect fxaaGreenLumaLowEffect;
        #if(WINDOWS || W8_1 || W10)
        internal FXAAEffect fxaaGreenLumaMediumEffect;
        internal FXAAEffect fxaaGreenLumaHighEffect;
        #endif

        public AntiAliasing(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(_graphicsDevice);

            CreateEffect();

            ResizeRenderTarget(_graphicsDevice.Viewport);
        }

        private void CreateEffect()
        {
            Viewport viewport = _graphicsDevice.Viewport;
            
            try // try to create a 9_3 shader.
            {
                fxaaGreenLumaLowEffect = new FXAAGreenLumaLowEffect(_graphicsDevice);
                SetEffectParameters(fxaaGreenLumaLowEffect, 0.5f, viewport);
            }
            catch (Exception ex1) { }

			#if(WINDOWS || W8_1 || W10)
            try
            {
                if (_graphicsDevice.GraphicsProfile >= GraphicsProfile.HiDef)          
                {
                    fxaaGreenLumaMediumEffect = new FXAAGreenLumaMediumEffect(_graphicsDevice);
                    fxaaGreenLumaHighEffect = new FXAAGreenLumaHighEffect(_graphicsDevice);
                    SetEffectParameters(fxaaGreenLumaMediumEffect, 0.5f, viewport);
                    SetEffectParameters(fxaaGreenLumaHighEffect, 0.5f, viewport);
                }
            }
            catch(Exception ex2) {}
			#endif

            return;
        }

        private void SetEffectParameters(FXAAEffect effect, float N, Viewport viewport)
        {
            effect.SetDefaultParameters(viewport.Width, viewport.Height);
            effect.AntialiasingEnabled = true;
        }

        private void ResizeRenderTarget(Viewport viewport)
        {
            int maxWidth = viewport.Width;
            int maxHeight = viewport.Height;
            int maxTexDim = (_graphicsDevice.GraphicsProfile>=GraphicsProfile.HiDef) ? 4096 : 2048;
            maxWidth = Math.Min(maxTexDim, maxWidth);
            maxHeight = Math.Min(maxTexDim, maxHeight);

            if (_renderTarget != null)
            {
                if (_renderTarget.Width == maxWidth && _renderTarget.Height == maxHeight) return;
                if (!_renderTarget.IsDisposed) _renderTarget.Dispose();
                _renderTarget = null;
            }
            if (_renderTarget == null)
            {
                _renderTarget = new RenderTarget2D(_graphicsDevice, maxWidth, maxHeight,
                    false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

                if (fxaaGreenLumaLowEffect!= null)
                {
                    fxaaGreenLumaLowEffect.SetDefaultParameters(maxWidth, maxHeight);
                }
                if (_graphicsDevice.GraphicsProfile >= GraphicsProfile.HiDef)
                {
                    fxaaGreenLumaMediumEffect.SetDefaultParameters(maxWidth, maxHeight);
                    fxaaGreenLumaHighEffect.SetDefaultParameters(maxWidth, maxHeight);
                }
            }
            return;
        }

        public void SetRenderTarget(Viewport viewport, Color color)
        {
            ResizeRenderTarget(viewport);
            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.Clear(color);

            int width = viewport.Width;
            int height = viewport.Height;
            int maxTexDim = (_graphicsDevice.GraphicsProfile >= GraphicsProfile.HiDef) ? 4096 : 2048;
            int maxWidth = Math.Min(maxTexDim, width);
            int maxHeight = Math.Min(maxTexDim, height);
            _graphicsDevice.Viewport = new Viewport(0, 0, width, height);
        }

        public void DrawRenderTarget(int antiAliasingLevel, Viewport viewport, bool clearRenderTarget)
        {
            FXAAEffect fxaaEffect = fxaaGreenLumaLowEffect;
            #if(WINDOWS || W8_1 || W10)
            if (antiAliasingLevel == 1) fxaaEffect = fxaaGreenLumaLowEffect;
            if (antiAliasingLevel == 2) fxaaEffect = fxaaGreenLumaMediumEffect;
            if (antiAliasingLevel == 3) fxaaEffect = fxaaGreenLumaHighEffect;
            #endif
            fxaaEffect.AntialiasingEnabled = (antiAliasingLevel != 0);

            if(clearRenderTarget)
                _graphicsDevice.SetRenderTarget(null);

            Rectangle srcRect;
            srcRect = new Rectangle(0, 0, (int)(_renderTarget.Width), (int)(_renderTarget.Height));
            
            Viewport oldViewport = _graphicsDevice.Viewport;
            _graphicsDevice.Viewport = viewport;

            Rectangle destRect = new Rectangle(0, 0, viewport.Width, viewport.Height);
            fxaaEffect.CurrentTechnique.Passes[0].Apply();
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, null, null, fxaaEffect);
            _spriteBatch.Draw(_renderTarget, destRect, srcRect, Color.White);
            _spriteBatch.End();

            _graphicsDevice.Viewport = oldViewport;
        }

    }
}
