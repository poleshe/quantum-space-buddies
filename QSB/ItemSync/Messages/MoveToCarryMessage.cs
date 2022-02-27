﻿using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.ItemSync.Messages
{
	internal class MoveToCarryMessage : QSBWorldObjectMessage<IQSBItem>
	{
		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			var itemType = WorldObject.GetItemType();

			player.HeldItem = WorldObject;
			var itemSocket = itemType switch
			{
				ItemType.Scroll => player.ScrollSocket,
				ItemType.SharedStone => player.SharedStoneSocket,
				ItemType.WarpCore => ((QSBWarpCoreItem)WorldObject).IsVesselCoreType()
					? player.VesselCoreSocket
					: player.WarpCoreSocket,
				ItemType.Lantern => player.SimpleLanternSocket,
				ItemType.DreamLantern => player.DreamLanternSocket,
				ItemType.SlideReel => player.SlideReelSocket,
				ItemType.VisionTorch => player.VisionTorchSocket,
				_ => player.ItemSocket,
			};
			WorldObject.PickUpItem(itemSocket);

			switch (itemType)
			{
				case ItemType.Scroll:
					player.AnimationSync.VisibleAnimator.SetTrigger("HoldScroll");
					break;
				case ItemType.WarpCore:
					if (((QSBWarpCoreItem)WorldObject).IsVesselCoreType())
					{
						player.AnimationSync.VisibleAnimator.SetTrigger("HoldAdvWarpCore");
					}
					else
					{
						player.AnimationSync.VisibleAnimator.SetTrigger("HoldWarpCore");
					}

					break;
				case ItemType.SharedStone:
					player.AnimationSync.VisibleAnimator.SetTrigger("HoldSharedStone");
					break;
				case ItemType.ConversationStone:
					player.AnimationSync.VisibleAnimator.SetTrigger("HoldItem");
					break;
				case ItemType.Lantern:
					player.AnimationSync.VisibleAnimator.SetTrigger("HoldLantern");
					break;
				case ItemType.SlideReel:
					break;
				case ItemType.DreamLantern:
					break;
				case ItemType.VisionTorch:
					break;
			}
		}
	}
}