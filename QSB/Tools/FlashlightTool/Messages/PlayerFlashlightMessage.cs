﻿using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;

namespace QSB.Tools.FlashlightTool.Messages;

public class PlayerFlashlightMessage : QSBMessage<bool>
{
	static PlayerFlashlightMessage()
	{
		GlobalMessenger.AddListener(OWEvents.TurnOnFlashlight, () => Handle(true));
		GlobalMessenger.AddListener(OWEvents.TurnOffFlashlight, () => Handle(false));
	}

	private static void Handle(bool on)
	{
		if (PlayerTransformSync.LocalInstance)
		{
			new PlayerFlashlightMessage(on).Send();
		}
	}

	private PlayerFlashlightMessage(bool on) => Value = on;

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.FlashlightActive = Value;
		player.FlashLight?.UpdateState(Value);
	}

	public override void OnReceiveLocal() =>
		QSBPlayerManager.LocalPlayer.FlashlightActive = Value;
}