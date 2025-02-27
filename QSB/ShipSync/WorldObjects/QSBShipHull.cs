﻿using QSB.Messaging;
using QSB.ShipSync.Messages.Hull;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ShipSync.WorldObjects;

internal class QSBShipHull : WorldObject<ShipHull>
{
	public override void SendInitialState(uint to)
	{
		if (AttachedObject._damaged)
		{
			this.SendMessage(new HullDamagedMessage { To = to });
		}
		else
		{
			this.SendMessage(new HullRepairedMessage { To = to });
		}

		this.SendMessage(new HullChangeIntegrityMessage(AttachedObject._integrity) { To = to });
	}

	public void SetDamaged()
	{
		if (AttachedObject._damaged)
		{
			return;
		}

		DebugLog.DebugWrite($"[HULL] {AttachedObject} Set damaged.");
		AttachedObject._damaged = true;
		AttachedObject.RaiseEvent(nameof(AttachedObject.OnDamaged), AttachedObject);
	}

	public void SetRepaired()
	{
		if (!AttachedObject._damaged)
		{
			return;
		}

		DebugLog.DebugWrite($"[HULL] {AttachedObject} Set repaired.");
		AttachedObject._damaged = false;
		AttachedObject.RaiseEvent(nameof(AttachedObject.OnRepaired), AttachedObject);
		var damageEffect = AttachedObject._damageEffect;
		damageEffect.SetEffectBlend(0f);
	}

	public void ChangeIntegrity(float newIntegrity)
	{
		if (OWMath.ApproxEquals(AttachedObject._integrity, newIntegrity))
		{
			return;
		}

		AttachedObject._integrity = newIntegrity;
		var damageEffect = AttachedObject._damageEffect;
		damageEffect.SetEffectBlend(1f - newIntegrity);
	}
}
