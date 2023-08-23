using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ImageViewer : UdonSharpBehaviour
{
    [UdonSynced]
    private int _playerId = -1;

    [UdonSynced]
    private VRCUrl _url;

    [SerializeField]
    private Renderer _renderer;

    [SerializeField]
    private Text _statusText;

    [SerializeField]
    private Text _nameText;

    [SerializeField]
    private Text _urlText;

    [SerializeField]
    private VRCUrlInputField _inputField;

    [SerializeField]
    private VRCUrlInputField _urlCopyInputField;

    [SerializeField]
    private GameObject _pixelModeOffObject;

    [SerializeField]
    private GameObject _pixelModeOnObject;

    private bool _enablePixelMode;

    private bool _isWorking;

    private bool _isSendUrlPending;

    private bool _isDownloaded;

    private float _downloadedTime;

    private string _playerName;

    private VRCUrl _currentUrl;

    private MaterialPropertyBlock _materialPropertyBlock;

    private VRCImageDownloader _imageDownloader;
    private IUdonEventReceiver _udonEventReceiver;

    private IVRCImageDownload _dl;

    private TextureInfo _textureInfo = new TextureInfo();

    private void Start()
    {
        _textureInfo.GenerateMipMaps = true;
        _textureInfo.WrapModeU = TextureWrapMode.Clamp;
        _textureInfo.WrapModeV = TextureWrapMode.Clamp;
        _textureInfo.AnisoLevel = 16;

        _imageDownloader = new VRCImageDownloader();
        _udonEventReceiver = (IUdonEventReceiver)this;
        _materialPropertyBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_materialPropertyBlock);
    }

    public void Update()
    {
        if (_isWorking)
        {
            if (_dl.State == VRCImageDownloadState.Pending)
            {
                if (_isDownloaded)
                {
                    float duration = Time.realtimeSinceStartup - _downloadedTime;
                    if (duration > 4)
                    {
                        _statusText.text = "<color=#F44>Download may failed.</color>";
                        _isWorking = false;
                    }
                }
                else
                {
                    if (_dl.Progress >= 1)
                    {
                        _statusText.text = "<color=#BBB>Processing Image...</color>";
                        _isDownloaded = true;
                        _downloadedTime = Time.realtimeSinceStartup;
                    }
                    else
                    {
                        _statusText.text = string.Format("<color=#BBB>Downloading... ({0:P})</color>", _dl.Progress);
                    }
                }
            }
            else
            {
                _statusText.text = _dl.State.ToString();
                _isWorking = false;
                if (_dl.State == VRCImageDownloadState.Error)
                {
                    _statusText.text = $"<color=#F44>[Error - {_dl.Error}]</color> {_dl.ErrorMessage}";
                }
            }
        }
    }

    public void _OnURLEndEdit()
    {
        if (_currentUrl != null && _currentUrl.ToString() == _inputField.GetUrl().ToString())
        {
            return;
        }
        _currentUrl = _inputField.GetUrl();
        _inputField.SetUrl(VRCUrl.Empty);
        _playerName = $"<color=#BFB>{Networking.LocalPlayer.displayName}</color>";

        _isSendUrlPending = true;
        LoadImage();
    }

    public void OnPostSerialization()
    {
        _playerName = GetPlayerName(_playerId);
        _nameText.text = _playerName;
    }

    public override void OnDeserialization()
    {
        if (_currentUrl != null && _currentUrl.ToString() == _url.ToString())
        {
            return;
        }
        _isSendUrlPending = false;
        _playerName = GetPlayerName(_playerId);

        _currentUrl = _url;
        LoadImage();
    }

    private void LoadImage()
    {
        _statusText.text = "";
        _nameText.text = _playerName;
        _urlText.text = _currentUrl.ToString();
        _urlCopyInputField.SetUrl(_currentUrl);
        bool urlEnpty = _currentUrl.ToString().Length == 0;
        _urlCopyInputField.gameObject.SetActive(!urlEnpty);
        if (urlEnpty)
        {
            _renderer.enabled = false;
            CheckSend();
            return;
        }
        _dl = _imageDownloader.DownloadImage(_currentUrl, _renderer.material, _udonEventReceiver, _textureInfo);

        _isWorking = true;
        _isDownloaded = false;
    }

    private void CheckSend()
    {
        if (_isSendUrlPending)
        {
            _url = _currentUrl;
            _playerId = Networking.LocalPlayer.playerId;
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            RequestSerialization();
            _isSendUrlPending = false;
        }
    }

    private string GetPlayerName(int playerId)
    {
        var player = VRCPlayerApi.GetPlayerById(playerId);
        return Utilities.IsValid(player) ? $"<color=#FB4>{player.displayName}</color>" : $"[ID {playerId}]";
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        CheckSend();
        int w = result.Result.width;
        int h = result.Result.height;
        _statusText.text = string.Format("{0}x{1} <color=#BBB>({2} {3:#,0} KB)</color>", w, h, result.Result.format, result.SizeInMemoryBytes / 1024f);

        _renderer.transform.localScale = new Vector3(w >= h ? 1 : (float)w / h, w <= h ? 1 : (float)h / w, 1);
        _renderer.transform.localPosition = new Vector3(0, w <= h ? 0 : (1 - (float)h / w) / -2, 0);
        _renderer.enabled = true;
        _isWorking = false;
    }

#if false
    public override void OnImageLoadError(IVRCImageDownload result)
    {
        _dl = result;
    }
#endif

    public void _OnClickPixelModeToggle()
    {
        _enablePixelMode = !_enablePixelMode;
        _pixelModeOffObject.SetActive(!_enablePixelMode);
        _pixelModeOnObject.SetActive(_enablePixelMode);

        _materialPropertyBlock.SetInt("_EnableSharp", _enablePixelMode ? 1 : 0);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }

    private void OnDestroy()
    {
        _imageDownloader.Dispose();
    }
}
