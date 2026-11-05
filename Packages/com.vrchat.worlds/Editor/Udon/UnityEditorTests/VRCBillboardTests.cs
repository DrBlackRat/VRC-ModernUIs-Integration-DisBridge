using System;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Components;
using Object = UnityEngine.Object;

namespace Tests
{
    public class VRCBillboardTests
    {
        private Action<Transform, VRCBillboardOptions> _previousRegister;

        private GameObject _billboardObject;
        private GameObject _vehicleObject;

        private int _registerCount;
        private VRCBillboardOptions _captured;

        [SetUp]
        public void Setup()
        {
            _previousRegister = VRCBillboard._Register;
            VRCBillboard._Register = (target, options) =>
            {
                _registerCount++;
                _captured = options;
            };

            _registerCount = 0;
            _captured = default;

            _billboardObject = new GameObject("Billboard");
            _vehicleObject = new GameObject("Vehicle");
        }

        [TearDown]
        public void TearDown()
        {
            VRCBillboard._Register = _previousRegister;

            Object.DestroyImmediate(_billboardObject);
            Object.DestroyImmediate(_vehicleObject);
        }

        [Test]
        public void UpFromMatchingTheBillboardCollapsesToWorldUp()
        {
            Transform billboard = _billboardObject.transform;

            VRCBillboard.Register(billboard, upFrom: billboard);

            string forwarded = _captured.UpFrom == null ? "null" : _captured.UpFrom.name;
            Assert.That(_registerCount, Is.EqualTo(1),
                $"Register should forward exactly one registration, forwarded {_registerCount}.");
            Assert.That(_captured.UpFrom, Is.Null,
                "An object cannot be its own up reference, so upFrom equal to the billboarded Transform should " +
                $"collapse to null for world up, but '{forwarded}' was forwarded.");
        }

        // Packing the arguments into the struct is the one hand-written mapping left in the chain. Each bool is
        // asserted true in one case and false in the other, so hardcoding either fails a case and swapping the
        // two fails both.
        [Test]
        public void OptionsArePackedUnchanged()
        {
            Transform billboard = _billboardObject.transform;
            Transform vehicle = _vehicleObject.transform;

            VRCBillboard.Register(billboard, VRCBillboardFacing.Up, allowTilt: true, smoothingFactor: 0.25f,
                upFrom: vehicle, matchViewpointRotation: false);

            string forwarded = _captured.UpFrom == null ? "null" : _captured.UpFrom.name;
            Assert.That(_captured.UpFrom, Is.SameAs(vehicle),
                $"A separate up reference must survive registration, but '{forwarded}' was packed instead of " +
                $"'{vehicle.name}'.");
            Assert.That(_captured.Facing, Is.EqualTo(VRCBillboardFacing.Up),
                $"facing should be packed unchanged, got {_captured.Facing}.");
            Assert.That(_captured.SmoothingFactor, Is.EqualTo(0.25f),
                $"smoothingFactor should be packed unchanged, got {_captured.SmoothingFactor}.");
            Assert.That(_captured.AllowTilt, Is.True,
                "allowTilt: true should be packed unchanged, got false.");
            Assert.That(_captured.MatchViewpointRotation, Is.False,
                "matchViewpointRotation: false should be packed unchanged, got true.");

            VRCBillboard.Register(billboard, allowTilt: false, matchViewpointRotation: true);

            Assert.That(_captured.AllowTilt, Is.False,
                "allowTilt: false should be packed unchanged, got true.");
            Assert.That(_captured.MatchViewpointRotation, Is.True,
                "matchViewpointRotation: true should be packed unchanged, got false.");
        }
    }
}
