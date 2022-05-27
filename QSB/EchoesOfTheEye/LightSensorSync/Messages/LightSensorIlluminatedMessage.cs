﻿using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

internal class LightSensorIlluminatedMessage : QSBWorldObjectMessage<QSBLightSensor, bool>
{
	public LightSensorIlluminatedMessage(bool illuminated) : base(illuminated) { }

	public override void OnReceiveRemote()
	{
		if (WorldObject.AttachedObject._illuminated == Data)
		{
			return;
		}

		WorldObject.AttachedObject._illuminated = Data;
		if (Data)
		{
			WorldObject.AttachedObject.OnDetectLight.Invoke();
		}
		else
		{
			WorldObject.AttachedObject.OnDetectDarkness.Invoke();
		}
	}
}
