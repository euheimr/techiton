using UnityEngine;

public class CameraMovement : MonoBehaviour {

    private int _maxAllowedMovement;
    // 0 for true, >0 for false
    private int _isMovementAllowed = 0;
    private Vector2 _screenSize;

    private bool _isStopped = false;

    void Start()
    {
        // Calculate maximum camera movement distance based on camera FOV and maximum height
        _maxAllowedMovement = Constants.TERRAIN_HALF_SIZE;// -cameraMovementMargin;
        _screenSize.x = Screen.width;
        _screenSize.y = Screen.height;
    }

	// Update is called once per frame
	void Update () {
        // Press space to block camera movement
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _isStopped = !_isStopped;
        }

        if (_isStopped)
            return;

        if (_isMovementAllowed > 0)
            return;

        Vector3 mousePos = Input.mousePosition;

        Vector3 cameraPos = transform.position + Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * Constants.CAMERA_ZOOM_SPEED;
        // Zoom camera along its forward axis
        if (cameraPos.y > Constants.CAMERA_MAX_HEIGHT || cameraPos.y < Constants.CAMERA_MIN_HEIGHT)
            cameraPos = transform.position;
        if (mousePos.x < _screenSize.x*Constants.CAMERA_MOVEMENT_MARGIN && mousePos.x > 0)
        {
            cameraPos -= Vector3.right * Constants.CAMERA_MOVEMENT_SPEED * (1 - mousePos.x/(_screenSize.x * Constants.CAMERA_MOVEMENT_MARGIN));
        }
        if (mousePos.x > Screen.width - _screenSize.x * Constants.CAMERA_MOVEMENT_MARGIN && mousePos.x < Screen.width)
        {
            cameraPos += Vector3.right * Constants.CAMERA_MOVEMENT_SPEED * (1-(Screen.width - mousePos.x) / (_screenSize.x * Constants.CAMERA_MOVEMENT_MARGIN));
        }
        if (mousePos.y < _screenSize.y * Constants.CAMERA_MOVEMENT_MARGIN && mousePos.y > 0)
        {
            cameraPos -= Vector3.forward *  Constants.CAMERA_MOVEMENT_SPEED * (1-mousePos.y / (_screenSize.y * Constants.CAMERA_MOVEMENT_MARGIN));
        }
        if (mousePos.y > Screen.height - _screenSize.y * Constants.CAMERA_MOVEMENT_MARGIN && mousePos.y < Screen.height)
        {
            cameraPos += Vector3.forward * Constants.CAMERA_MOVEMENT_SPEED * (1-(Screen.height - mousePos.y) / (_screenSize.y * Constants.CAMERA_MOVEMENT_MARGIN));
        }

        // Check terrain boundaries
        if (cameraPos.x > -_maxAllowedMovement && cameraPos.x < _maxAllowedMovement &&
            cameraPos.z > -_maxAllowedMovement && cameraPos.z < _maxAllowedMovement)
            transform.position = cameraPos;
    }

    public void LockCameraMovement()
    {
        _isMovementAllowed++;
    }

    public void UnlockCameraMovement()
    {
        _isMovementAllowed--;
    }

}
