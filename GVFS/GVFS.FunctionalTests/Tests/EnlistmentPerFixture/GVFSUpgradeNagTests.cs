﻿using GVFS.FunctionalTests.FileSystemRunners;
using GVFS.FunctionalTests.Tools;
using GVFS.Tests.Should;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace GVFS.FunctionalTests.Tests.EnlistmentPerFixture
{
    [TestFixture]
    [Category(Categories.FullSuiteOnly)]
    [Category(Categories.Windows)]
    public class UpgradeNagTests : TestsWithEnlistmentPerFixture
    {
        private const string GVFSInstallerName = "SetupGVFS.1.0.18234.1.exe";
        private const string GitInstallerName = "Git-2.17.1.gvfs.2.5.g2962052-64-bit.exe";

        private string upgradeDirectory;
        private FileSystemRunner fileSystem;

        public UpgradeNagTests()
        {
            this.fileSystem = new SystemIORunner();
            this.upgradeDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create),
                "GVFS",
                "GVFS.Upgrade",
                "Downloads");
        }

        [TestCase]
        [Category(Categories.Windows)]
        public void NoNagWhenUpgradeNotAvailable()
        {
            this.EmptyDownloadDirectory();

            ProcessResult result = GitHelpers.InvokeGitAgainstGVFSRepo(
                this.Enlistment.RepoRoot,
                "status");

            string.IsNullOrEmpty(result.Errors).ShouldBeTrue();
        }

        [TestCase]
        [Category(Categories.Windows)]
        public void NagWhenUpgradeAvailable()
        {
            this.CreateUpgradeInstallers();

            ProcessResult result = GitHelpers.InvokeGitAgainstGVFSRepo(
                this.Enlistment.RepoRoot,
                "status");

            result.Errors.ShouldContain(new string[] 
                {
                    "A newer version of GVFS is available.",
                    "Run \"gvfs upgrade\" on \"Command Prompt\" as Administrator to install."
                });

            this.EmptyDownloadDirectory();
        }

        private void EmptyDownloadDirectory()
        {
            if (Directory.Exists(this.upgradeDirectory))
            {
                Directory.Delete(this.upgradeDirectory, recursive: true);
            }

            Directory.CreateDirectory(this.upgradeDirectory);
            Directory.Exists(this.upgradeDirectory).ShouldBeTrue();
            Directory.EnumerateFiles(this.upgradeDirectory).Any().ShouldBeFalse();
        }

        private void CreateUpgradeInstallers()
        {
            string gvfsInstallerPath = Path.Combine(this.upgradeDirectory, GVFSInstallerName);
            string gitInstallerPath = Path.Combine(this.upgradeDirectory, GitInstallerName);

            this.EmptyDownloadDirectory();

            this.fileSystem.CreateEmptyFile(gvfsInstallerPath);
            this.fileSystem.CreateEmptyFile(gitInstallerPath);
            this.fileSystem.FileExists(gvfsInstallerPath).ShouldBeTrue();
            this.fileSystem.FileExists(gitInstallerPath).ShouldBeTrue();
        }
    }
}
