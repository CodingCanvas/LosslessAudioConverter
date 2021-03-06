﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LosslessAudioConverter.ViewModel;
using FluentAssertions;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using LosslessAudioConverter.Commands;

namespace AcceptanceTests
{
    [TestClass]
    public class DirectorySelectionViewModelTests
    {
        DirectorySelectionViewModel _dirViewModel;

        readonly static string TestDataRootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles");
        readonly static string TestSourceDir = Path.Combine(TestDataRootDir, @"SourceFiles");
        readonly static string TestTargetDir = Path.Combine(TestDataRootDir, @"TargetFiles");

        static List<string> testSourceFiles = new List<string>()
        {
            //"./SourceFiles/Recording.flac",
            //"./SourceFiles/Recording.m4a",
            //"./SourceFiles/Recording.mp3",
            //"./SourceFiles/Recording.ogg",
            //"./SourceFiles/Recording.opus",
            //"./SourceFiles/Recording.wav",
            //"./SourceFiles/Folder 1/cover.png",
            //"./SourceFiles/Folder 1/Recording.flac",
            //"./SourceFiles/Folder 1/Recording.m4a",
            //"./SourceFiles/Folder 1/Recording.mp3",
            //"./SourceFiles/Folder 1/Recording.ogg",
            //"./SourceFiles/Folder 1/Recording.opus",
            //"./SourceFiles/Folder 1/Recording.wav",
            //"./SourceFiles/Folder 1/SomeText.txt",
        };

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            DirectoryInfo targetDir = Directory.Exists(TestTargetDir)
                ? new DirectoryInfo(TestTargetDir)
                : Directory.CreateDirectory(TestTargetDir);

            var targetFiles = targetDir.EnumerateFiles("*.*", SearchOption.AllDirectories).ToList();
            var targetSubDirectories = targetDir.EnumerateDirectories().ToList();

            foreach (var targetFile in targetFiles)
            {
                targetFile.Delete();
            }
            foreach (var targetSubDir in targetSubDirectories)
            {
                targetSubDir.Delete(recursive: true);
            }

            testSourceFiles.AddRange(Directory.EnumerateFiles(TestSourceDir, "*.*", SearchOption.AllDirectories));
        }


        [TestInitialize]
        public void TestInitialize()
        {
            _dirViewModel = new DirectorySelectionViewModel();
        }

        [TestMethod]
        public void VerifyDefaultSourceDirectoryIsSystemMusicFolder()
        {
            var defaultMusicDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic, Environment.SpecialFolderOption.None);

            _dirViewModel.SourceDirectory.Should().Be(defaultMusicDirectory, "because the default source folder should be the system deafult");
            _dirViewModel.TargetDirectory.Should().BeEmpty("because no target directory has been selected.");
        }

        [TestMethod]
        public void VerifyDirectoriesCanBeSet()
        {
            _dirViewModel.SourceDirectory.Should().NotBe(TestTargetDir);
            _dirViewModel.TargetDirectory.Should().BeEmpty();

            SetDefaultConversionDirectories();

            _dirViewModel.SourceDirectory.Should().Be(TestSourceDir);
            _dirViewModel.TargetDirectory.Should().Be(TestTargetDir);
        }

        private void SetDefaultConversionDirectories()
        {
            _dirViewModel.SourceDirectory = TestSourceDir;
            _dirViewModel.TargetDirectory = TestTargetDir;
        }

        [TestMethod]
        public void VerifyFlacAndWavFilesAreConverted()
        {
            testSourceFiles.Count.Should().BeGreaterThan(1); //sanity check

            SetDefaultConversionDirectories();
            var srcDir = new DirectoryInfo(TestSourceDir);
            var targetDir = new DirectoryInfo(TestTargetDir);

            srcDir.EnumerateDirectories()
                .Select(x => x.FullName).ToList()
                .Should().HaveCount(1);
            srcDir.EnumerateFiles("*.*", SearchOption.AllDirectories)
                .Should().HaveSameCount(testSourceFiles);
            
            targetDir.EnumerateDirectories().Should().BeEmpty();
            targetDir.EnumerateFiles("*.*", SearchOption.AllDirectories).Should().BeEmpty();

            IAsyncCommand convertAsyncCommand = _dirViewModel.StartConversionCommand as IAsyncCommand;
            convertAsyncCommand.Should().NotBeNull();
            convertAsyncCommand.CanExecute(null).Should().BeTrue();

            var conversionTask = convertAsyncCommand.ExecuteAsync(null);
            conversionTask.Wait(TimeSpan.FromSeconds(60));

            conversionTask.IsCompleted.Should().BeTrue();
            conversionTask.IsCanceled.Should().BeFalse();
            conversionTask.IsFaulted.Should().BeFalse();

            var expectedOutput = testSourceFiles
                .Select(x => x.Replace(".flac", ".ogg"))
                .Select(x => x.Replace(".wav", ".ogg"))
                .Select(x => x.Replace(TestSourceDir, TestTargetDir))
                .Select(x => x.ToLower())
                .ToList();

            var actualSrc = srcDir.EnumerateFiles("*.*", SearchOption.AllDirectories);
            var actualOutput = targetDir.EnumerateFiles("*.*", SearchOption.AllDirectories).ToList();

            actualOutput.Select(x => x.FullName.ToLower()).Should().BeEquivalentTo(expectedOutput);

            actualSrc.Sum(x => x.Length)
                .Should().BeGreaterThan(actualOutput.Sum(x => x.Length), 
                "because the output directory should ALWAYS have a smaller size than the output.");
        }
    }
}
