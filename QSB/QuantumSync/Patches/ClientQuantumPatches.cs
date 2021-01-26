﻿using QSB.Patches;

namespace QSB.QuantumSync.Patches
{
	public class ClientQuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		public override void DoPatches() => QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumMoon>("ChangeQuantumState", typeof(ClientQuantumPatches), nameof(ReturnFalsePatch));

		public static bool ReturnFalsePatch() => false;
	}
}