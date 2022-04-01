﻿using HarmonyLib;
using QSB.ItemSync.Messages;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.ItemSync.Patches;

[HarmonyPatch(typeof(ItemTool))]
internal class ItemToolPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.MoveItemToCarrySocket))]
	public static void MoveItemToCarrySocket(OWItem item)
	{
		var qsbItem = item.GetWorldObject<IQSBItem>();
		QSBPlayerManager.LocalPlayer.HeldItem = qsbItem;
		qsbItem.SendMessage(new MoveToCarryMessage());
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.SocketItem))]
	public static void SocketItem(ItemTool __instance, OWItemSocket socket)
	{
		var item = __instance._heldItem;
		QSBPlayerManager.LocalPlayer.HeldItem = null;
		new SocketItemMessage(SocketMessageType.Socket, socket, item).Send();
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.StartUnsocketItem))]
	public static void StartUnsocketItem(OWItemSocket socket)
	{
		var item = socket.GetSocketedItem();
		var qsbItem = item.GetWorldObject<IQSBItem>();
		QSBPlayerManager.LocalPlayer.HeldItem = qsbItem;
		new SocketItemMessage(SocketMessageType.StartUnsocket, socket, item).Send();
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.CompleteUnsocketItem))]
	public static void CompleteUnsocketItem(ItemTool __instance)
	{
		var item = __instance._heldItem;
		new SocketItemMessage(SocketMessageType.CompleteUnsocket, null, item).Send();
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.DropItem))]
	public static bool DropItem(ItemTool __instance, RaycastHit hit, OWRigidbody targetRigidbody, IItemDropTarget customDropTarget)
	{
		Locator.GetPlayerAudioController().PlayDropItem(__instance._heldItem.GetItemType());
		var gameObject = hit.collider.gameObject;
		var component = gameObject.GetComponent<ISectorGroup>();
		Sector sector = null;

		while (component == null && gameObject.transform.parent != null)
		{
			gameObject = gameObject.transform.parent.gameObject;
			component = gameObject.GetComponent<ISectorGroup>();
		}

		if (component != null)
		{
			sector = component.GetSector();
			if (sector == null && component is SectorCullGroup sectorCullGroup)
			{
				var controllingProxy = sectorCullGroup.GetControllingProxy();
				if (controllingProxy != null)
				{
					sector = controllingProxy.GetSector();
				}
			}
		}

		var parent = customDropTarget == null
			? targetRigidbody.transform
			: customDropTarget.GetItemDropTargetTransform(hit.collider.gameObject);
		var qsbItem = __instance._heldItem.GetWorldObject<IQSBItem>();
		__instance._heldItem.DropItem(hit.point, hit.normal, parent, sector, customDropTarget);
		customDropTarget?.AddDroppedItem(hit.collider.gameObject, __instance._heldItem);

		__instance._heldItem = null;
		QSBPlayerManager.LocalPlayer.HeldItem = null;

		Locator.GetToolModeSwapper().UnequipTool();

		var qsbDropItem = QSBWorldSync.GetWorldObjects<MonoBehaviourWorldObject>()
			.FirstOrDefault(x => (IItemDropTarget)x.AttachedObject == customDropTarget);

		qsbItem.SendMessage(new DropItemMessage(hit.point, hit.normal, parent, sector, qsbDropItem, targetRigidbody));

		return false;
	}
}
