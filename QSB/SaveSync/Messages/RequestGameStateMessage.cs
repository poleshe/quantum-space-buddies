﻿using QSB.ConversationSync.Messages;
using QSB.ConversationSync.Patches;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using System.Linq;

namespace QSB.SaveSync.Messages;

/// <summary>
/// always sent to host
/// </summary>
internal class RequestGameStateMessage : QSBMessage
{
	public RequestGameStateMessage() => To = 0;

	public override void OnReceiveRemote() => Delay.RunFramesLater(100, () =>
	{
		if (!QSBPlayerManager.PlayerExists(From))
		{
			// player was kicked
			return;
		}

		new GameStateMessage(From).Send();

		var gameSave = PlayerData._currentGameSave;

		var factSaves = gameSave.shipLogFactSaves;
		foreach (var item in factSaves)
		{
			new ShipLogFactSaveMessage(item.Value).Send();
		}

		var dictConditions = gameSave.dictConditions;
		var dictConditionsToSend = dictConditions.Where(x => ConversationPatches.PersistentConditionsToSync.Contains(x.Key));
		foreach (var item in dictConditionsToSend)
		{
			new PersistentConditionMessage(item.Key, item.Value).Send();
		}
	});
}