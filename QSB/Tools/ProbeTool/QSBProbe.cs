﻿using QSB.Player;
using QSB.Utility;
using UnityEngine;

namespace QSB.Tools.ProbeTool;

public class QSBProbe : MonoBehaviour, ILightSource
{
	public delegate void SurveyorProbeEvent();
	public delegate void RetrieveEvent(float retrieveLength);

	[SerializeField]
	private OWLight2[] _illuminationCheckLights;

	public event SurveyorProbeEvent OnLaunchProbe;
	public event SurveyorProbeEvent OnAnchorProbe;
	public event SurveyorProbeEvent OnUnanchorProbe;
	public event SurveyorProbeEvent OnRetrieveProbe;
	public event SurveyorProbeEvent OnProbeDestroyed;
	public event RetrieveEvent OnStartRetrieveProbe;

	private GameObject _detectorObj;
	private RulesetDetector _rulesetDetector;
	private SingularityWarpEffect _warpEffect;
	private bool _isRetrieving;
	private PlayerInfo _owner;
	private bool _anchored;

	public RulesetDetector GetRulesetDetector()
		=> _rulesetDetector;

	private void Awake()
	{
		_detectorObj = GetComponentInChildren<RulesetDetector>().gameObject;
		_rulesetDetector = _detectorObj.GetComponent<RulesetDetector>();
		_warpEffect = GetComponentInChildren<SingularityWarpEffect>();
		_warpEffect.OnWarpComplete += OnWarpComplete;
		_isRetrieving = false;
	}

	private void Start() => gameObject.SetActive(false);

	protected void OnDestroy() => _warpEffect.OnWarpComplete -= OnWarpComplete;

	public void SetOwner(PlayerInfo player)
	{
		if (_owner != null)
		{
			DebugLog.ToConsole($"Warning - Trying to set owner of probe that already has an owner!", OWML.Common.MessageType.Warning);
		}

		_owner = player;
	}

	private void OnWarpComplete() => Deactivate();

	public bool IsRetrieving()
		=> IsLaunched() && _isRetrieving;

	public bool IsLaunched()
		=> gameObject.activeSelf;

	public bool IsAnchored()
		=> IsLaunched() && _anchored;

	public void HandleEvent(ProbeEvent probeEvent)
	{
		if (_owner == null)
		{
			DebugLog.ToConsole($"Error - Trying to handle event on probe with no owner.", OWML.Common.MessageType.Error);
			return;
		}

		switch (probeEvent)
		{
			case ProbeEvent.Launch:
				_anchored = false;

				gameObject.SetActive(true);
				transform.position = _owner.ProbeLauncher.transform.position;
				transform.rotation = _owner.ProbeLauncher.transform.rotation;

				if (OnLaunchProbe == null)
				{
					DebugLog.ToConsole($"Warning - OnLaunchProbe is null!", OWML.Common.MessageType.Warning);
					break;
				}

				OnLaunchProbe();
				break;
			case ProbeEvent.Anchor:
				_anchored = true;

				if (OnAnchorProbe == null)
				{
					DebugLog.ToConsole($"Warning - OnAnchorProbe is null!", OWML.Common.MessageType.Warning);
					break;
				}

				OnAnchorProbe();
				break;
			case ProbeEvent.Unanchor:
				_anchored = false;
				OnUnanchorProbe();
				break;
			case ProbeEvent.Retrieve:
				_anchored = false;
				if (OnRetrieveProbe == null)
				{
					DebugLog.ToConsole($"Warning - OnRetrieveProbe is null!", OWML.Common.MessageType.Warning);
					break;
				}

				OnRetrieveProbe();
				break;
			case ProbeEvent.Destroy:
				_anchored = false;
				Destroy(gameObject);

				if (OnProbeDestroyed == null)
				{
					DebugLog.ToConsole($"Warning - OnProbeDestroyed is null!", OWML.Common.MessageType.Warning);
					break;
				}

				OnProbeDestroyed();
				break;
			case ProbeEvent.Invalid:
			default:
				DebugLog.ToConsole($"Warning - Unknown/Invalid probe event.", OWML.Common.MessageType.Warning);
				break;
		}
	}

	private void Deactivate()
	{
		transform.localScale = Vector3.one;
		gameObject.SetActive(false);
		_isRetrieving = false;
	}

	public void OnStartRetrieve(float duration)
	{
		if (!_isRetrieving)
		{
			_isRetrieving = true;
			_warpEffect.WarpObjectOut(duration);

			if (OnStartRetrieveProbe == null)
			{
				DebugLog.ToConsole($"Warning - OnStartRetrieveProbe is null!", OWML.Common.MessageType.Warning);
				return;
			}

			OnStartRetrieveProbe(duration);
		}
	}

	public bool CheckIlluminationAtPoint(Vector3 point, float buffer = 0f, float maxDistance = float.PositiveInfinity)
	{
		for (var i = 0; i < _illuminationCheckLights.Length; i++)
		{
			if (_illuminationCheckLights[i].CheckIlluminationAtPoint(point, buffer, maxDistance))
			{
				return true;
			}
		}

		return false;
	}

	public LightSourceType GetLightSourceType() => LightSourceType.PROBE;
	public OWLight2[] GetLights() => _illuminationCheckLights;
}