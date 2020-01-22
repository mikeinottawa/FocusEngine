using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Core;
using Xenko.Core.Annotations;
using Xenko.Shaders;
using Xenko.Rendering.Materials;
using static Xenko.Rendering.Voxels.VoxelAttributeEmissionOpacity;

namespace Xenko.Rendering.Voxels
{
    [DataContract(DefaultMemberMode = DataMemberMode.Default)]
    public class VoxelLayoutBase
    {
        public enum StorageFormats
        {
            R10G10B10A2,
            RGBA8,
            RGBA16F,
        };

        [NotNull]
        public IVoxelStorageMethod StorageMethod { get; set; } = new VoxelStorageMethodIndirect();

        public StorageFormats StorageFormat { get; set; } = StorageFormats.RGBA16F;

        Graphics.PixelFormat StorageFormatToPixelFormat()
        {
            Graphics.PixelFormat format = Graphics.PixelFormat.R16G16B16A16_Float;
            switch (StorageFormat)
            {
                case StorageFormats.RGBA8:
                    format = Graphics.PixelFormat.R8G8B8A8_UNorm;
                    break;
                case StorageFormats.R10G10B10A2:
                    format = Graphics.PixelFormat.R10G10B10A2_UNorm;
                    break;
                case StorageFormats.RGBA16F:
                    format = Graphics.PixelFormat.R16G16B16A16_Float;
                    break;
            }
            return format;
        }

        [Display("Max Brightness (non float format)")]
        public float maxBrightness = 10.0f;




        //Per-Layout Settings
        protected virtual int LayoutCount { get; set; } = 1;
        protected virtual ShaderClassSource Writer { get; set; } = new ShaderClassSource("VoxelIsotropicWriter_Float4");
        protected virtual ShaderClassSource Sampler { get; set; } = new ShaderClassSource("VoxelIsotropicSampler");
        protected virtual string ApplierKey { get; set; } = "Isotropic";




        protected IVoxelStorageTexture storageTex;

        virtual public int PrepareLocalStorage(VoxelStorageContext context, IVoxelStorage storage)
        {
            return StorageMethod.PrepareLocalStorage(context, storage, 4, LayoutCount);
        }
        virtual public void PrepareOutputStorage(VoxelStorageContext context, IVoxelStorage storage)
        {
            storage.UpdateTexture(context, ref storageTex, StorageFormatToPixelFormat(), LayoutCount);
        }
        virtual public void ClearOutputStorage()
        {
            storageTex = null;
        }

        virtual public void PostProcess(RenderDrawContext drawContext, string shader)
        {
            storageTex.PostProcess(drawContext, shader);
        }


        

        protected ValueParameterKey<float> BrightnessInvKey;
        protected ObjectParameterKey<Xenko.Graphics.Texture> DirectOutput;

        virtual public ShaderSource GetVoxelizationShader(List<IVoxelModifierEmissionOpacity> modifiers)
        {
            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(Writer);
            StorageMethod.Apply(mixin);
            foreach (var modifier in modifiers)
            {
                if (!modifier.Enabled) continue;

                ShaderSource applier = modifier.GetApplier(ApplierKey);
                if (applier != null)
                    mixin.AddCompositionToArray("Modifiers", applier);
            }
            return mixin;
        }
        virtual public void ApplyVoxelizationParameters(ParameterCollection parameters, List<IVoxelModifierEmissionOpacity> modifiers)
        {
            if (StorageFormat != StorageFormats.RGBA16F)
                parameters.Set(BrightnessInvKey, 1.0f / maxBrightness);
            else
                parameters.Set(BrightnessInvKey, 1.0f);

            storageTex.ApplyVoxelizationParameters(DirectOutput, parameters);
        }




        protected ValueParameterKey<float> BrightnessKey;

        virtual public ShaderSource GetSamplingShader()
        {
            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(Sampler);
            mixin.AddComposition("storage", storageTex.GetSamplingShader());
            return mixin;
        }
        virtual public void ApplySamplingParameters(VoxelViewContext viewContext, ParameterCollection parameters)
        {
            if (StorageFormat != StorageFormats.RGBA16F)
                parameters.Set(BrightnessKey, maxBrightness);
            else
                parameters.Set(BrightnessKey, 1.0f);

            storageTex.ApplySamplingParameters(viewContext, parameters);
        }
    }
}