using SpellFlinger.Scriptables;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class CameraController : Singleton<CameraController>
    {
        [SerializeField] private int _zoomLevels = 0;
        [SerializeField] private float _angularSpeed = 0;
        [SerializeField] private float _maxAngleDelta = 0;
        [SerializeField] private float _maxCameraAngle = 0;
        [SerializeField] private Transform _shootPoint = null;
        private Transform _endTarget = null;
        private Transform _startTarget = null;
        private int _currentZoom;
        private float _zoomPercentage;
        private Vector3 _worldPositionNoCollision;
        private LayerMask _layerMask;
        private bool _cameraEnabled = false;
        private Vector3 _oldRotation;
        private bool _initialized = false;

        public Transform ShootPoint => _shootPoint;
        public bool CameraLock { get; set; }

        public bool CameraEnabled
        {
            get => _cameraEnabled;
            set
            {
                if (CameraLock) value = false;
                _cameraEnabled = value;
                if (_cameraEnabled)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        private void LateUpdate()
        {

            Debug.DrawLine(transform.position, transform.position + transform.forward * 50, Color.green);

            if (!_initialized) return;
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                CameraEnabled = !CameraEnabled;
            }

            if (!_cameraEnabled) return;

            float angle = - SensitivitySettingsScriptable.Instance.UpDownSensitivity * Input.GetAxis("Mouse Y");
            angle = Mathf.Clamp(angle, -_maxCameraAngle / _maxAngleDelta, _maxCameraAngle / _maxAngleDelta);
            _oldRotation = _endTarget.localEulerAngles;
            _endTarget.Rotate(angle, 0, 0);

            if (Mathf.Abs(_endTarget.localEulerAngles.x) > _maxCameraAngle && Mathf.Abs(_endTarget.localEulerAngles.x) < 360 - _maxCameraAngle)
            {
                if (Mathf.Abs(_endTarget.localEulerAngles.x) < 360 / 2) _oldRotation.x = _maxCameraAngle;
                else _oldRotation.x = 360 - _maxCameraAngle;
                _endTarget.localEulerAngles = _oldRotation;
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0f && _currentZoom < _zoomLevels)
            {
                _currentZoom++;
                _zoomPercentage = (float)_currentZoom / _zoomLevels;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0f && _currentZoom > 1)
            {
                _currentZoom--;
                _zoomPercentage = (float)_currentZoom / _zoomLevels;
            }

            transform.position = Vector3.Lerp(_endTarget.position, _startTarget.position, _zoomPercentage);
            transform.LookAt(_endTarget);
            _worldPositionNoCollision = transform.position;

            RaycastHit hit;
            if (Physics.SphereCast(_endTarget.position, 0.5f, _worldPositionNoCollision - _endTarget.position, out hit,
                Vector3.Distance(_worldPositionNoCollision, _endTarget.position), _layerMask))
            {
                transform.position = Vector3.Lerp(_endTarget.position, hit.point, 0.975f);
            }
        }

        public void Init(Transform startTarget, Transform endTarget)
        {
            _initialized = true;
            _startTarget = startTarget;
            _endTarget = endTarget;
            transform.localPosition = _startTarget.localPosition;
            _currentZoom = _zoomLevels;
            _zoomPercentage = 1f;
            transform.LookAt(_endTarget);
            _layerMask = LayerMask.GetMask("Ground");
            CameraEnabled = true;
        }
    }
}