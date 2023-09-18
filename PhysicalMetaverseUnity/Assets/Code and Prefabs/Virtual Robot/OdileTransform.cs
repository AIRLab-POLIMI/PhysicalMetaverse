using Unity.Netcode;
using UnityEngine;
using System.Linq;

/// <summary>
/// An example network serializer with both server and owner authority.
/// Love Tarodev
/// </summary>
public class OdileTransform : NetworkBehaviour {
    /// <summary>
    /// A toggle to test the difference between owner and server auth.
    /// </summary>
    [SerializeField] private bool _serverAuth;
    [SerializeField] private float _cheapInterpolationTime = 0.1f;
    //list of joints transforms
    [SerializeField] private Transform[] _jointsTransforms;

    private Vector3[] _jointsAngles;

    private NetworkVariable<OdileNetworkState> _playerState;
    //private Rigidbody transform;

    private void Awake() {
        //transform = GetComponent<Rigidbody>();

        var permission = _serverAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        _playerState = new NetworkVariable<OdileNetworkState>(writePerm: permission);
        //init jointsangles to size of jointstransforms
        _jointsAngles = new Vector3[_jointsTransforms.Length];
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner){
            Destroy(transform.GetComponent<RobotController>());
            //get camera in children of this gameobject
            Destroy(GetComponentInChildren<Camera>());
            //destroy listener
            Destroy(GetComponentInChildren<AudioListener>());
        }
    }

    private void Update() {
        if (IsOwner) TransmitState();
        else ConsumeState();
    }

    #region Transmit State

    private void TransmitState() {
        var state = new OdileNetworkState {
            Position = transform.position,
            Rotation = transform.rotation.eulerAngles
        };

        if (IsServer || !_serverAuth)
            _playerState.Value = state;
        else
            TransmitStateServerRpc(state);
    }

    [ServerRpc]
    private void TransmitStateServerRpc(OdileNetworkState state) {
        _playerState.Value = state;
    }

    #endregion

    #region Interpolate State

    private Vector3 _posVel;
    private float _rotVelY;

    private void ConsumeState() {
    // Here you'll find the cheapest, dirtiest interpolation you'll ever come across. Please do better in your game
    //transform.MovePosition(Vector3.SmoothDamp(transform.position, _playerState.Value.Position, ref _posVel, _cheapInterpolationTime));
    transform.position = Vector3.SmoothDamp(transform.position, _playerState.Value.Position, ref _posVel, _cheapInterpolationTime);

    transform.rotation = Quaternion.Euler(
        0, Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, _playerState.Value.Rotation.y, ref _rotVelY, _cheapInterpolationTime), 0);

    //log entire _jointsAngles
    //Debug.Log("Joints angles: " + _jointsAngles);
    // Also update the rotations of the joints
    /*for (int i = 0; i < _jointsTransforms.Length; i++) {
        _jointsTransforms[i].rotation = Quaternion.Euler(_playerState.Value.JointsRotations[i]);
    }*/
}

    #endregion

    private struct OdileNetworkState : INetworkSerializable {
        private float _posX, _posZ, _posY;
        private short _rotY;
        private Vector3[] _jointsRotations;

        internal Vector3 Position {
            get => new(_posX, _posY, _posZ);
            set {
                _posX = value.x;
                _posY = value.y;
                _posZ = value.z;
            }
        }

        internal Vector3 Rotation {
        get => new(0, _rotY, 0);
        set => _rotY = (short)value.y;
    }

        internal Vector3[] JointsRotations {
            get => _jointsRotations;
            set => _jointsRotations = value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref _posX);
            serializer.SerializeValue(ref _posY);
            serializer.SerializeValue(ref _posZ);

            serializer.SerializeValue(ref _rotY);
        }

    }
}