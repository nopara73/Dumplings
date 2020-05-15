using System;
using System.Collections.Generic;
using System.Text;

namespace Dumplings.Cli
{
    public enum Command
    {
        Sync,   // Syncs the Scanner files from where it was left off.
        Resync, // Resync the Scanner files from Wasabi's launch.
        Check   // Cross checks the Scanner files to make sure of no bugs.
    }
}
