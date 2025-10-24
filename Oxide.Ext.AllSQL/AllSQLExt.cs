using Oxide.Core;
using Oxide.Core.Extensions;
using System;
namespace Oxide.Ext.AllSQL
{
    public sealed class AllSQLExt : Extension
    {
        public override string Name => "AllSQL";
        public override string Author => "RFC1920";
        public override VersionNumber Version => new(1, 0, 1);

        private static readonly bool debug = true;

        public AllSQLExt(ExtensionManager manager) : base(manager)
        {
            LogDebug("AllSQL Extension loaded.");
        }

        private static void LogDebug(string debugTxt)
        {
            if (debug) Interface.Oxide.LogDebug(debugTxt);
        }

        public override void Load()
        {
            Interface.Oxide.OnFrame(new Action<float>(OnFrame));
        }
        private void OnFrame(float delta)
        {
            object[] objArray = new object[] { delta };
        }
        public override void OnModLoad()
        {
        }

        public override void OnShutdown()
        {
        }
    }
}
