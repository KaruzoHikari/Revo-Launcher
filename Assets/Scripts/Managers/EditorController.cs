using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Animations;
using DG.Tweening;
using Misc;
using SFB;
using SimpleFileBrowser;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Object = System.Object;
using Random = UnityEngine.Random;

public class EditorController : MonoBehaviour
{

    [Title("Prefabs")]
    // For the vectors, when we create them, we set their contentType to float or int
    // For Vector2 or Vector3, we add new YZ fields by cloning the X one.
    public GameObject imageSelectorPrefab;
    public GameObject videoSelectorPrefab;
    public GameObject animationEditPrefab;
    public GameObject vectorPrefab;
    public GameObject colorPrefab;
    public GameObject dropdownPrefab;
    public GameObject titlePrefab;
    public GameObject textPrefab;
    public GameObject currentAnimationPrefab;
    public GameObject buttonAddAnimationPrefab;
    public GameObject buttonAddImagePrefab;
    public GameObject animationAddPrefab;
    public GameObject foldoutPrefab;
    public GameObject booleanPrefab;
    public GameObject stringPrefab;
    public GameObject imageListPrefab;
    public GameObject imageAddPrefab;
    public GameObject videoAddPrefab;
    public GameObject iconAddPrefab;
    public GameObject audioPrefab;
    public GameObject channelEditor;

    [Title("Textures")]
    public Texture checkmark;
    public Texture cross;

    [Title("Game Variables")]
    public GameObject decoyBanner;
    public GameObject decoyIcon;
    public RectTransform optionsHolder;
    public GameObject exitButton;
    public GameObject backButton;
    public GameObject finishButton;
    public GameObject deleteButton;
    public GameObject timerHolder;

    public ChannelAnimation currentChannelAnimation;
    public AnimatedImage currentImage;
    public Animation currentAnimation;

    public static EditorController _instance;

    private TextMeshProUGUI currentAudioName;
    private TextMeshProUGUI currentImageName;

    private void Awake()
    {
        _instance = this;
    }

    [Button("Open Editor")]
    public void OpenEditor(ChannelAnimation channelAnimation)
    {
        StartCoroutine(_OpenEditor(channelAnimation));
    }

    private IEnumerator _OpenEditor(ChannelAnimation channelAnimation)
    {
        SettingsController._instance.ExitSettings(false);

        yield return new WaitForSeconds(1.5f * FadeController.GetSpeedMultiplier());
        channelEditor.SetActive(true);
        currentChannelAnimation = channelAnimation;
        AnimationController._instance.ChangeCurrentAnimation(currentChannelAnimation);
        SetNewMenu(MENU.CHANNEL_ANIMATION);
        
        if (!PREFS.HasOpenedEditor.GetBool())
        {
            PREFS.HasOpenedEditor.SetBool(true);
            PopupController.ShowPopup("popup.editorwelcome");
        }
    }

    public void ExitEditor()
    {
        AnimationController._instance.Resume();
        AnimationController._instance.ChangeCurrentAnimation(null);
        currentChannelAnimation?.DeleteLoadingFolder();
        currentChannelAnimation = null;
        SettingsController._instance.OpenSettings();
        GetGridObject()?.SetActive(false);
    }

    public void ShowDecoy()
    {
        PreviewController._instance.ShowDecoy(currentChannelAnimation, true);
    }

    public void HideDecoy()
    {
        PreviewController._instance.HideDecoy(true);
    }

    public void ClickedInfo()
    {
        // for now it shows and hides the timer
        timerHolder.SetActive(!timerHolder.activeSelf);
    }

    public void ClickedGrid()
    {
        if (PreviewController._instance.mainDecoy != null && PreviewController._instance.mainDecoy.activeInHierarchy)
        {
            GameObject grid = GetGridObject();
            grid.SetActive(!grid.activeSelf);
        }
    }

    public void SetGridStatus(bool shown)
    {
        GameObject grid = GetGridObject();
        if (grid is not null)
        {
            grid.SetActive(shown);
        }
    }

    private GameObject GetGridObject()
    {
        GameObject decoy = PreviewController._instance.GetCurrentDecoy();
        if (decoy == null)
        {
            return null;
        }
        
        Transform source = decoy == decoyIcon ? decoy.transform.Find("Channel") : decoy.transform;
        return source.Find("ChannelObjects").Find("Grid").gameObject;
    }

    public void ClickedPause()
    {
        if (PreviewController._instance.mainDecoy != null && PreviewController._instance.mainDecoy.activeInHierarchy)
        {
            if (AnimationController._instance.paused)
            {
                AnimationController._instance.Resume();
            }
            else
            {
                AnimationController._instance.Pause();
            }
        }
    }

    private void SetupButtons(bool finish, bool back/*, bool exit*/, bool delete)
    {
        finishButton.SetActive(finish);
        backButton.SetActive(back);
        //exitButton.SetActive(exit);
        deleteButton.SetActive(delete);
    }

    private void FinishChannelAnim()
    {
        PopupController.ShowPopup("popup.savechannel", () =>
        {
            bool saved = currentChannelAnimation.Save();
            if (!saved)
            {
                return;
            }
            
            ExitEditor();
            PopupController.ClosePopup();
        }, () =>
        {
            PopupController.ShowPopup("popup.discardchannel",
                () =>
                {
                    ExitEditor();
                    PopupController.ClosePopup();
                }, PopupController.ClosePopup);
        });
    }

    private void DeleteChannelAnim()
    {
        PopupController.ShowPopup("popup.deletechannel", () =>
        {
            ChannelController._instance.DeleteChannelAnimation(currentChannelAnimation);
            ExitEditor();
            PopupController.ClosePopup();
        }, PopupController.ClosePopup);
    }

    private void DeleteAnimation()
    {
        PopupController.ShowPopup("popup.deleteanim", () =>
        {

            currentImage.DeleteAnimation(currentAnimation);
            currentAnimation = null;
            SetNewMenu(MENU.ANIMATION_LIST);
            PopupController.ClosePopup();
        }, PopupController.ClosePopup);
    }

    private void DeleteImage()
    {
        PopupController.ShowPopup("popup.deleteimage", () =>
        {
            currentChannelAnimation.DeleteImage(currentImage);
            currentImage = null;
            SetNewMenu(MENU.IMAGE_LIST);
            PopupController.ClosePopup();
        }, PopupController.ClosePopup);
    }

    private void SetButtonCallback(Button button, UnityAction action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void SetNewMenu(MENU menu)
    {
        // Here we setup the buttons
        Button buttonBack = backButton.GetComponent<Button>();
        Button buttonFinish = finishButton.GetComponent<Button>();
        Button buttonDelete = deleteButton.GetComponent<Button>();

        switch (menu)
        {
            case MENU.CHANNEL_ANIMATION:
            {
                SetupChannelAnimationMenu();
                
                SetupButtons(true, false, true);
                SetButtonCallback(buttonFinish, FinishChannelAnim);
                SetButtonCallback(buttonDelete, DeleteChannelAnim);
                break;
            }
            
            case MENU.IMAGE_LIST:
            {
                SetupImageListMenu();
                
                SetupButtons(false, true, false);
                SetButtonCallback(buttonBack, () => SetNewMenu(MENU.CHANNEL_ANIMATION));
                break;
            }
            
            case MENU.IMAGE_EDIT:
            {
                ClearOptions();
                OptionsSpawner.SetupOptions(currentImage,optionsHolder);
                if (currentImage.isVideo)
                {
                    SetupVideoOptions();
                }
                else
                {
                    SetupImageOptions();
                }
                
                SetupButtons(true, false, true);
                SetButtonCallback(buttonFinish, () => SetNewMenu(MENU.IMAGE_LIST));
                SetButtonCallback(buttonDelete, DeleteImage);
                break;
            }
            case MENU.ANIMATION_LIST:
            {
                SetupCurrentAnimationList();
                
                SetupButtons(false, true, false);
                SetButtonCallback(buttonBack, () => SetNewMenu(MENU.IMAGE_EDIT));
                break;
            }
            
            case MENU.ANIMATION_CREATE:
            {
                SetupCreateAnimationList();
                
                SetupButtons(false, true, false);
                // todo add animation validation to the back button (start time later than object start, etc)
                SetButtonCallback(buttonBack, () => SetNewMenu(MENU.ANIMATION_LIST));
                break;
            }
            
            case MENU.ANIMATION_EDIT:
            {
                ClearOptions();
                SpawnTitle(AnimationLister.GetInfo(currentAnimation).name);
                OptionsSpawner.SetupOptions(currentAnimation,optionsHolder);
                
                SetupButtons(true, false, true);
                SetButtonCallback(buttonFinish, () => SetNewMenu(MENU.ANIMATION_LIST));
                SetButtonCallback(buttonDelete, DeleteAnimation);
                break;
            }
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(optionsHolder);
    }
    
    private void SetupCurrentAnimationList()
    {
        ClearOptions();
        SpawnTitle("editor.title.listofanims.simple");
        foreach (Animation animation in currentImage.animationList)
        {
            SpawnAnimation_Edit(animation,optionsHolder);
        }

        GameObject addButton = Instantiate(buttonAddAnimationPrefab, optionsHolder);
        addButton.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            SetNewMenu(MENU.ANIMATION_CREATE);
        });
    }

    private void SetupChannelAnimationMenu()
    {
        ClearOptions();
        SpawnTitle("editor.title.channelprops");
        OptionsSpawner.SetupOptions(currentChannelAnimation,optionsHolder);
        
        // If it's a banner animation, we list the Audio
        if (currentChannelAnimation.type == CHANNELTYPE.BANNER)
        {
            SpawnAudioButton();
        }
        
        SpawnVoid(300f);

        GameObject addButton = Instantiate(buttonAddImagePrefab, optionsHolder);
        addButton.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            SetNewMenu(MENU.IMAGE_LIST);
        });
    }

    private void SetupImageListMenu()
    {
        ClearOptions();
        SpawnTitle("editor.title.listofimages");
        foreach (AnimatedImage image in currentChannelAnimation.images)
        {
            SpawnImage_Edit(image);
        }

        GameObject addButton = Instantiate(imageAddPrefab, optionsHolder);
        addButton.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            CreateNewImage();
        });
        
        GameObject addIconButton = Instantiate(iconAddPrefab, optionsHolder);
        addIconButton.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            CreateNewImage(true);
        });
        
        GameObject videoButton = Instantiate(videoAddPrefab, optionsHolder);
        videoButton.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            CreateNewVideo();
        });
    }

    private void SpawnAudioButton()
    {
        GameObject audioSelection = Instantiate(audioPrefab, optionsHolder);
        GameObject findButton = audioSelection.transform.Find("NameAndChange").Find("Button").gameObject; 
        findButton.GetComponent<Button>().onClick.AddListener(RequestNewAudio);
        findButton.GetComponent<LongClickButton>().onLongClick.AddListener(() =>
            {
                PopupController.ShowPopup("popup.deleteaudio", () =>
                {
                    currentAudioName.text = "None";
                    currentChannelAnimation.audioClip = null;
                    currentChannelAnimation.audioPath = null;
                    PopupController.ClosePopup();
                }, PopupController.ClosePopup);
            });
        
        currentAudioName = audioSelection.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>();
        currentAudioName.text = currentChannelAnimation.audioClip == null ? "None" : currentChannelAnimation.audioClip.name;
    }
    
    private void RequestNewAudio()
    {
        FileManager.RequestFile("Choose a new sound for the Channel Banner", FILETYPE.AUDIO, path =>
        {
            if (path == null)
            {
                Debug.Log( "Operation cancelled" );
            }
            else
            {
                // we move it to a readable path
                string loadedPath = currentChannelAnimation.MoveToLoadingFolder(path);
                string finalPath = Application.isMobilePlatform ? "file://" + loadedPath : loadedPath;
                AudioController.LoadAudio(finalPath, audioClip => OnAudioLoaded(audioClip,loadedPath));
            }
        });
    }

    private void OnAudioLoaded(AudioClip clip, string path)
    {
        Debug.Log("Loaded audio");
        if (clip == null || currentAudioName == null || currentAudioName.IsDestroyed() || currentAudioName.gameObject == null)
        {
            return;
        }

        currentChannelAnimation.audioPath = path;
        currentChannelAnimation.audioClip = clip;
        currentAudioName.text = clip.name;
    }
    
    private void SetupCreateAnimationList()
    {
        ClearOptions();
        SpawnTitle("editor.title.listofanims.full");
        foreach (AnimationLister info in AnimationLister.GetAllAnimations())
        {
            if (!info.isHidden)
            {
                SpawnAnimation_Create(info,optionsHolder);
            }
        }
    }
    
    public void CreateNewImage(bool isIcon = false)
    {
        currentImage = new AnimatedImage();
        currentImage.isIcon = isIcon;
        currentImage.isCodeTexture = isIcon;
        currentImage.finishedLoading = isIcon;
        currentChannelAnimation.AddImage(currentImage);
        SetNewMenu(MENU.IMAGE_EDIT);
    }
    
    public void CreateNewVideo()
    {
        currentImage = new AnimatedImage();
        currentImage.isVideo = true;
        currentImage.finishedLoading = true;
        currentChannelAnimation.AddImage(currentImage);
        SetNewMenu(MENU.IMAGE_EDIT);
    }

    public void CreateNewAnimation(AnimationLister lister)
    {
        currentAnimation = (Animation) Activator.CreateInstance(lister.classType);
        currentImage.AddAnimation(currentAnimation);
        SetNewMenu(MENU.ANIMATION_EDIT);
    }

    private void SetupImageOptions()
    {
        GameObject imageSelection = Instantiate(imageSelectorPrefab, optionsHolder);
        imageSelection.transform.SetAsFirstSibling();

        if (currentImage.isCodeTexture)
        {
            imageSelection.transform.Find("NameAndChange").Find("Button").gameObject.SetActive(false);   
        }
        else
        {
            imageSelection.transform.Find("NameAndChange").Find("Button").gameObject.GetComponent<Button>().onClick
                .AddListener(() => RequestImage(imageSelection));
        }
        RawImage rawImage = imageSelection.transform.Find("Image").gameObject.GetComponent<RawImage>();
        if (currentImage.isGif && currentImage.HasGifFrames())
        {
            rawImage.texture = currentImage.gifFrames[0];
        }
        else
        {
            rawImage.texture = currentImage.image;
        }
        
        currentImageName = imageSelection.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>();
        currentImageName.text = string.IsNullOrEmpty(currentImage.imageName) ? "None" : currentImage.imageName;

        if (currentImage.isIcon || currentImage.isCodeTexture)
        {
            rawImage.texture = AnimationController._instance.iconDefaultTexture;
            currentImageName.text = TextController.GetTranslation("editor.title.appicon");
        }
        else if ((currentImage.isGif && !currentImage.HasGifFrames()) || (!currentImage.isGif && currentImage.image == null))
        {
            RequestImage(imageSelection);
        }
        
        GameObject animationEditor = Instantiate(animationEditPrefab, optionsHolder);
        animationEditor.transform.SetAsLastSibling();
        animationEditor.transform.Find("Button").gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            SetNewMenu(MENU.ANIMATION_LIST);
        });
    }

    private void RequestImage(GameObject imageSelection)
    {
        FileManager.RequestFile("Choose a new image to add to the Channel", FILETYPE.IMAGES_WITH_GIF,
            path => SetImage(imageSelection, path));
    }

    private void SetImage(GameObject settingsIcon, string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            string newPath = currentChannelAnimation.MoveToLoadingFolder(path);
            currentImage.filePath = newPath;
            bool gif = Path.GetExtension(path).Equals(".gif");
            if (gif)
            {
                currentImage.isGif = true;
                currentImage.image = null;
                currentImage.LoadGif();
            }
            else
            {
                // We load the image data
                byte[] rawData = FileBrowserHelpers.ReadBytesFromFile(path);
                Texture2D tex = new Texture2D(2, 2); // Empty texture
                tex.LoadImage(rawData, true);

                // And then we apply it to the image
                currentImage.isGif = false;
                currentImage.SetImageManually(tex);
                settingsIcon.transform.Find("Image").gameObject.GetComponent<RawImage>().texture = tex;
            }
            currentImage.imageName = StaticUtils.SanitizeString(FileManager.GetFileName(path));
            currentImageName.text = currentImage.imageName;
            SetNewMenu(MENU.IMAGE_EDIT);
        }
    }
    
    private void SetupVideoOptions()
    {
        if (!currentImage.isVideo)
        {
            // wat
            return;
        }
        
        GameObject imageSelection = Instantiate(videoSelectorPrefab, optionsHolder);
        imageSelection.transform.SetAsFirstSibling();
        
        imageSelection.transform.Find("NameAndChange").Find("Button").gameObject.GetComponent<Button>().onClick
                .AddListener(() => RequestNewVideo(imageSelection));

        currentImageName = imageSelection.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>();
        currentImageName.text = string.IsNullOrEmpty(currentImage.imageName) ? "None" : currentImage.imageName;
        
        if (string.IsNullOrEmpty(currentImage.filePath))
        {
            RequestNewVideo(imageSelection);
        }
        
        GameObject animationEditor = Instantiate(animationEditPrefab, optionsHolder);
        animationEditor.transform.SetAsLastSibling();
        animationEditor.transform.Find("Button").gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            SetNewMenu(MENU.ANIMATION_LIST);
        });
    }

    private void RequestNewVideo(GameObject settingsIcon)
    {
        FileManager.RequestFile("Choose a new video to add to the channel", FILETYPE.VIDEO, path =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                SetVideo(settingsIcon, path);
            }
        });
    }

    private void SetVideo(GameObject settingsIcon, string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            // For now we just redirect to the location of the video.
            // When saving the animation, we change this to redirect to the actual decomp path
            
            string newPath = currentChannelAnimation.MoveToLoadingFolder(path);
            currentImage.filePath = newPath;
            currentImage.imageName = StaticUtils.SanitizeString(FileManager.GetFileName(newPath));
            currentImageName.text = currentImage.imageName;
        }
    }

    public void ClearOptions()
    {
        if (optionsHolder.childCount > 0)
        {
            for (int i = 0; i < optionsHolder.childCount; i++)
            {
                Destroy(optionsHolder.GetChild(i).gameObject);
            }
        }
        TabController.ClearFields();
    }

    private void SpawnTitle(string id)
    {
        OptionsSpawner.SummonTitle(optionsHolder, id, false);
    }

    private void SpawnVoid(float height)
    {
        GameObject spawned = new GameObject("Void_" + height, typeof(RectTransform));
        spawned.transform.SetParent(optionsHolder);
        spawned.GetComponent<RectTransform>().sizeDelta = new Vector2(height, 100);
    }
    
    private void SpawnAnimation_Create(AnimationLister info, RectTransform parent)
    {
        GameObject spawned = OptionsSpawner.SpawnPrefab(animationAddPrefab,info.name,info.description, parent);

        spawned.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            CreateNewAnimation(info);
        });
    }
    
    private void SpawnAnimation_Edit(Animation animation, RectTransform parent)
    {
        AnimationLister info = AnimationLister.GetInfo(animation);
        string name = info.name.Replace(" Animation", "");

        GameObject spawned = OptionsSpawner.SpawnPrefab(currentAnimationPrefab, name, info.description, parent);

        spawned.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            currentAnimation = animation;
            SetNewMenu(MENU.ANIMATION_EDIT);
        });
        spawned.transform.Find("Button").GetComponent<LongClickButton>().onLongClick.AddListener(() =>
        {
            PopupController.ShowPopup("popup.cloneanim",
                () =>
                {
                    Animation cloned = animation.DeepClone();
                    animation.animInfo.AddAnimation(cloned);
                    currentAnimation = cloned;
                    SetNewMenu(MENU.ANIMATION_EDIT);
                    PopupController.ClosePopup();
                },
                PopupController.ClosePopup);
        });

        spawned.transform.Find("Values").Find("Start").Find("Text").GetComponent<TextMeshProUGUI>().text =
            animation.startTime.ToString();
        spawned.transform.Find("Values").Find("End").Find("Text").GetComponent<TextMeshProUGUI>().text =
            animation.endTime.ToString();
    }

    private void SpawnImage_Edit(AnimatedImage image)
    {
        GameObject spawned = Instantiate(imageListPrefab, optionsHolder);

        spawned.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            currentImage = image;
            SetNewMenu(MENU.IMAGE_EDIT);
        });
        spawned.transform.Find("Button").GetComponent<LongClickButton>().onLongClick.AddListener(() =>
        {
            PopupController.ShowPopup("popup.cloneimage",
                () =>
                {
                    AnimatedImage deepClonedImage = image.DeepClone();
                    image.channelAnimation.AddImage(deepClonedImage);
                    SetNewMenu(MENU.IMAGE_LIST);
                    PopupController.ClosePopup();
                },
                PopupController.ClosePopup);
        });

        if (image.isIcon)
        {
            spawned.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = TextController.GetTranslation("editor.title.appicon");
            spawned.transform.Find("Image").GetComponent<RawImage>().texture = AnimationController._instance.iconDefaultTexture;
        }
        else
        {
            RawImage rawImage = spawned.transform.Find("Image").GetComponent<RawImage>();
            rawImage.texture = image.GetThumbnail();
            spawned.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = image.imageName;
        }

        if (!image.enabled)
        {
            spawned.transform.Find("Image").Find("DisabledOverlay").gameObject.SetActive(true);
        }
    }

    private enum MENU
    {
        CHANNEL_ANIMATION, IMAGE_LIST, IMAGE_EDIT, ANIMATION_LIST, ANIMATION_CREATE, ANIMATION_EDIT
    }
    
    [Button("Add AnimatedImageCover")]
    public void BUTTON_AddAnimatedImageCover()
    {
        currentImage = new AnimatedImageCover();
        currentImage.isCodeTexture = true;
        currentImage.imageName = "ArtCover";
        currentChannelAnimation.AddImage(currentImage);
        SetNewMenu(MENU.IMAGE_EDIT);
    }
    
    [Button("Add AnimatedImageGame")]
    public void BUTTON_AddAnimatedImageGame()
    {
        currentImage = new AnimatedImageGame();
        currentImage.isCodeTexture = true;
        currentImage.imageName = "FrontCover";
        currentChannelAnimation.AddImage(currentImage);
        SetNewMenu(MENU.IMAGE_EDIT);
    }
    
    [Button("Add AnimatedImageText")]
    public void BUTTON_AddAnimatedImageText()
    {
        currentImage = new AnimatedImageText();
        currentImage.isCodeTexture = true;
        currentImage.imageName = "GameTitle";
        currentChannelAnimation.AddImage(currentImage);
        SetNewMenu(MENU.IMAGE_EDIT);
    }

    [Button("Add BoxRotation")]
    public void BUTTON_AddBoxRotation()
    {
        // ROTATION
        BoxArtRotationAnimation rotationAnimation = new BoxArtRotationAnimation();
        currentImage.AddAnimation(rotationAnimation);
        currentAnimation = rotationAnimation;
        SetNewMenu(MENU.ANIMATION_EDIT);
    }
}
