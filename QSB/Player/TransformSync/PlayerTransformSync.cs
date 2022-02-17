﻿using OWML.Common;
using QSB.Messaging;
using QSB.Player.Messages;
using QSB.PlayerBodySetup.Local;
using QSB.PlayerBodySetup.Remote;
using QSB.Syncs.Sectored.Transforms;
using QSB.Utility;
using UnityEngine;

namespace QSB.Player.TransformSync
{
	public class PlayerTransformSync : SectoredTransformSync
	{
		protected override bool IsPlayerObject => true;
		protected override bool AllowInactiveAttachedObject => true;

		private Transform _visibleCameraRoot;
		private Transform _networkCameraRoot => gameObject.transform.GetChild(0);

		// todo? stick root might be the thing that moves instead of roasting system. one of them doesn't, i just don't know which
		private Transform _visibleRoastingSystem;
		private Transform _networkRoastingSystem => gameObject.transform.GetChild(1);
		private Transform _networkStickRoot => _networkRoastingSystem.GetChild(0);

		private Transform _visibleStickPivot;
		private Transform _networkStickPivot => _networkStickRoot.GetChild(0);

		private Transform _visibleStickTip;
		private Transform _networkStickTip => _networkStickPivot.GetChild(0);

		protected Vector3 _cameraPositionVelocity;
		protected Quaternion _cameraRotationVelocity;
		protected Vector3 _pivotPositionVelocity;
		protected Quaternion _pivotRotationVelocity;
		protected Vector3 _tipPositionVelocity;
		protected Quaternion _tipRotationVelocity;
		protected Vector3 _roastingPositionVelocity;
		protected Quaternion _roastingRotationVelocity;

		public override void OnStartClient()
		{
			var player = new PlayerInfo(this);
			QSBPlayerManager.PlayerList.SafeAdd(player);
			base.OnStartClient();
			QSBPlayerManager.OnAddPlayer?.Invoke(Player);
			DebugLog.DebugWrite($"Create Player : id<{Player.PlayerId}>", MessageType.Info);
		}

		public override void OnStartLocalPlayer() => LocalInstance = this;

		public override void OnStopClient()
		{
			// TODO : Maybe move this to a leave event...? Would ensure everything could finish up before removing the player
			QSBPlayerManager.OnRemovePlayer?.Invoke(Player);
			base.OnStopClient();
			Player.HudMarker?.Remove();
			QSBPlayerManager.PlayerList.Remove(Player);
			DebugLog.DebugWrite($"Remove Player : id<{Player.PlayerId}>", MessageType.Info);
		}

		protected override void Uninit()
		{
			base.Uninit();

			if (isLocalPlayer)
			{
				Player.IsReady = false;
				new PlayerReadyMessage(false).Send();
			}
		}

		protected override Transform InitLocalTransform()
			=> LocalPlayerCreation.CreatePlayer(
				Player,
				SectorDetector,
				out _visibleCameraRoot,
				out _visibleRoastingSystem,
				out _visibleStickPivot,
				out _visibleStickTip);

		protected override Transform InitRemoteTransform()
			=> RemotePlayerCreation.CreatePlayer(
				Player,
				out _visibleCameraRoot,
				out _visibleRoastingSystem,
				out _visibleStickPivot,
				out _visibleStickTip);

		protected override void GetFromAttached()
		{
			base.GetFromAttached();
			if (!ReferenceTransform)
			{
				return;
			}

			GetFromChild(_visibleStickPivot, _networkStickPivot);
			GetFromChild(_visibleStickTip, _networkStickTip);
			GetFromChild(_visibleCameraRoot, _networkCameraRoot);
			GetFromChild(_visibleRoastingSystem, _networkRoastingSystem);
		}

		protected override void ApplyToAttached()
		{
			base.ApplyToAttached();
			if (!ReferenceTransform)
			{
				return;
			}

			ApplyToChild(_visibleStickPivot, _networkStickPivot, ref _pivotPositionVelocity, ref _pivotRotationVelocity);
			ApplyToChild(_visibleStickTip, _networkStickTip, ref _tipPositionVelocity, ref _tipRotationVelocity);
			ApplyToChild(_visibleCameraRoot, _networkCameraRoot, ref _cameraPositionVelocity, ref _cameraRotationVelocity);
			ApplyToChild(_visibleRoastingSystem, _networkRoastingSystem, ref _roastingPositionVelocity, ref _roastingRotationVelocity);
		}

		private static void GetFromChild(Transform visible, Transform network)
		{
			network.localPosition = visible.localPosition;
			network.localRotation = visible.localRotation;
		}

		private static void ApplyToChild(Transform visible, Transform network, ref Vector3 positionVelocity, ref Quaternion rotationVelocity)
		{
			visible.localPosition = Vector3.SmoothDamp(visible.localPosition, network.localPosition, ref positionVelocity, SmoothTime);
			visible.localRotation = QuaternionHelper.SmoothDamp(visible.localRotation, network.localRotation, ref rotationVelocity, SmoothTime);
		}

		protected override void OnRenderObject()
		{
			if (!QSBCore.DebugSettings.DrawLines
				|| !IsValid
				|| !ReferenceTransform)
			{
				return;
			}

			base.OnRenderObject();

			Popcron.Gizmos.Cube(ReferenceTransform.TransformPoint(_networkRoastingSystem.position), ReferenceTransform.TransformRotation(_networkRoastingSystem.rotation), Vector3.one / 4, Color.red);
			Popcron.Gizmos.Cube(ReferenceTransform.TransformPoint(_networkStickPivot.position), ReferenceTransform.TransformRotation(_networkStickPivot.rotation), Vector3.one / 4, Color.red);
			Popcron.Gizmos.Cube(ReferenceTransform.TransformPoint(_networkStickTip.position), ReferenceTransform.TransformRotation(_networkStickTip.rotation), Vector3.one / 4, Color.red);
			Popcron.Gizmos.Cube(ReferenceTransform.TransformPoint(_networkCameraRoot.position), ReferenceTransform.TransformRotation(_networkCameraRoot.rotation), Vector3.one / 4, Color.red);

			Popcron.Gizmos.Cube(_visibleRoastingSystem.position, _visibleRoastingSystem.rotation, Vector3.one / 4, Color.magenta);
			Popcron.Gizmos.Cube(_visibleStickPivot.position, _visibleStickPivot.rotation, Vector3.one / 4, Color.blue);
			Popcron.Gizmos.Cube(_visibleStickTip.position, _visibleStickTip.rotation, Vector3.one / 4, Color.yellow);
			Popcron.Gizmos.Cube(_visibleCameraRoot.position, _visibleCameraRoot.rotation, Vector3.one / 4, Color.grey);
		}

		protected override bool CheckReady() => base.CheckReady()
			&& (Locator.GetPlayerTransform() || AttachedTransform);

		public static PlayerTransformSync LocalInstance { get; private set; }

		protected override bool UseInterpolation => true;
	}
}
