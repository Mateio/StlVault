﻿using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StlVault.Util.Logging;

namespace StlVault.Services
{
    internal class PreviewImageStore : IPreviewImageStore
    {
        private readonly string _dataPath;
        private static readonly ILogger Logger = UnityLogger.Instance;

        public PreviewImageStore([NotNull] string dataPath)
        {
            _dataPath = dataPath ?? throw new ArgumentNullException(nameof(dataPath));
        }
        
        private string PreviewImagePath => Path.Combine(_dataPath, "PreviewImages");

        private string GetFileName(string fileHash)
        {
            // var (x, y, z) = rotation.GetRoundedValues();
            // var fileName = $"{fileHash}_{x}_{y}_{z}.jpg";

            var fileName = $"{fileHash}.jpg";
            foreach (var illegal in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(illegal, '-');
            }

            return Path.Combine(PreviewImagePath, fileName);
        }

        public async Task<byte[]> LoadPreviewAsync(string fileHash)
        {
            var fileName = GetFileName(fileHash);
            return await Task.Run(ReadImageBytes);

            byte[] ReadImageBytes()
            {
                try
                {
                    return File.ReadAllBytes(fileName);
                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task StorePreviewAsync(string fileHash, byte[] imageData)
        {
            var fileName = GetFileName(fileHash);
            await Task.Run(StoreImageBytes);

            void StoreImageBytes()
            {
                try
                {
                    var directory = Path.GetDirectoryName(fileName);
                    if (directory == null) return;

                    Directory.CreateDirectory(directory);

                    File.WriteAllBytes(fileName, imageData);
                }
                catch (Exception ex)
                {
                    Logger.Debug("Failed to store image {0}: {1}", fileName, ex.Message);
                }
            }
        }
    }
}