using System;
using System.Globalization;
using Animations;
using Misc;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ShopViews
{
    public class RatingShopView : ShopView
    {
        public int currentPage = 1;
        private int currentMaxPage;
        private Transform content;
        private string animationId;
        private bool isReview;
        private ContentType contentType;

        public RatingShopView(GameObject view) : base(view, () => {  })
        {
            content = view.transform.Find("Content").Find("Scroll View").Find("Viewport").Find("Content");
        }

        private async void FindFlaggedReviews()
        {
            // First we check if we're a mod
            bool areWeMod = await _instance.SendRequest(WebRequestController.SendGetRank()) >= UserRank.MOD;
            if (!areWeMod)
            {
                ClickedBack();
            }

            // Now we download the reviews
            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendGetFlaggedReviews(currentPage));
            JObject json = response.GetJsonResponse();
            if (!response.isNetworkError && !response.isHttpError)
            {
                // In this case the query was successful
                SetMaxPage((int) json["pages"]);
                JArray array = (JArray) json["reviews"];
                foreach (JToken anim in array)
                {
                    SpawnReview(anim, areWeMod);
                }
            }
            else
            {
                switch (response.responseCode)
                {
                    case 404:
                    {
                        // No pending-review rating was found
                        PopupController.ShowPopup("popup.mod.noflaggedreviews", ClickedBack);
                        break;
                    }
                }
            }
        }

        public async void Show(string animationId, ContentType type, bool isReview = false, int newPage = 1)
        {
            base.Show();
            this.currentPage = newPage;
            this.animationId = animationId;
            this.isReview = isReview;
            contentType = type;

            foreach (Transform child in content)
            {
                if (!child.gameObject.Equals(_instance.addReviewButton) &&
                    !child.gameObject.Equals(_instance.editReviewButton))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            if (string.IsNullOrEmpty(animationId))
            {
                FindFlaggedReviews();
                return;
            }

            if (!PREFS.HasOpenedRatings.GetBool())
            {
                PREFS.HasOpenedRatings.SetBool(true);
                PopupController.ShowPopup("popup.ratingswelcome");
            }
            
            // First we check if we're a mod
            bool areWeMod = await _instance.SendRequest(WebRequestController.SendGetRank()) >= UserRank.MOD;

            // Then we check if we already have an animation (only on the first page)
            _instance.addReviewButton.SetActive(false);
            _instance.editReviewButton.SetActive(false);
            UnityWebRequest reviewCheck = await _instance.SendRequest(WebRequestController.SendGetReview(contentType, animationId, ShopController.GetUserId()));
            bool found = !reviewCheck.isNetworkError && !reviewCheck.isHttpError;
            _instance.addReviewButton.SetActive(!found && newPage == 1);
            _instance.editReviewButton.SetActive(found && newPage == 1);

            // Now we download the reviews
            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendGetReviews(contentType, animationId, currentPage));
            JObject json = response.GetJsonResponse();
            if (!response.isNetworkError && !response.isHttpError)
            {
                // In this case the query was successful
                SetMaxPage((int) json["pages"]);
                JArray array = (JArray) json["reviews"];
                foreach (JToken anim in array)
                {
                    SpawnReview(anim, areWeMod);
                }
            }

            // no need to check for 404 cause you could be the first review
        }

        private void SpawnReview(JToken jToken, bool areWeMod)
        {
            // We spawn the name and the author
            string id = jToken["id"].ToString();
            string userName = jToken["username"].ToString();
            int rating = (int) jToken["rating"];
            string description = jToken["description"].ToString();

            Transform prefab = GameObject.Instantiate(_instance.reviewPrefab, content).transform;
            prefab.Find("Title").Find("Username").Find("Title").GetComponent<TextMeshProUGUI>().text = userName;
            prefab.Find("Review").Find("Title").GetComponent<TextMeshProUGUI>().text = description;
            prefab.Find("Title").Find("Stars").Find("Title").GetComponent<TextMeshProUGUI>().text = rating.ToString();

            Color color = Color.Lerp(ThemeController.GetColor(ThemeColor.HubReviewsMinStars), ThemeController.GetColor(ThemeColor.HubReviewsMaxStars), rating / 5f);
            prefab.GetComponent<Image>().color = color;
            
            // We setup the click-to-reveal-whole-review thing
            prefab.GetComponent<Button>().onClick.AddListener(() =>
            {
                PopupController.ShowPopup($"{userName}:\n\n{description}");
            });

            // Finally we setup the delete button if we're a mod
            // Or we add a report system for regular users
            prefab.GetComponent<LongClickButton>().onLongClick.AddListener(() =>
            {
                if (areWeMod)
                {
                    PopupController.ShowPopup("popup.mod.deletereview", () => { DeleteReview(id); },
                        PopupController.ClosePopup, new[] {userName});
                }
                else
                {
                    PopupController.ShowPopup("popup.reportreview", () => { ReportReview(id); },
                        PopupController.ClosePopup, new[] {userName});
                }
            });
            
            // And if we're in the reviews menu, we setup the approve and search buttons
            if (isReview)
            {
                GameObject flag = prefab.Find("Review").Find("Flag").gameObject;
                flag.SetActive(true);
                flag.GetComponent<Button>().onClick.AddListener(() => ApproveReview(id));
                
                string originId = jToken["originId"].ToString();
                GameObject navigate = prefab.Find("Review").Find("Navigate").gameObject;
                navigate.SetActive(true);
                navigate.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _instance.browseContentView.Show(null,originId,null,null,0,contentType);
                });
            }
        }

        private async void ApproveReview(string id)
        {
            if (_instance.IsHandlingRequest())
            {
                return;
            }

            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendApproveRating(id));
            if (!response.isNetworkError && !response.isHttpError)
            {
                Show(animationId,contentType,isReview,currentPage);
            }
        }

        private async void DeleteReview(string id)
        {
            if (_instance.IsHandlingRequest())
            {
                return;
            }

            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendDeleteReview(id));
            if (!response.isNetworkError && !response.isHttpError)
            {
                PopupController.ShowPopup("popup.reviewdeleted", () =>
                {
                    Show(animationId,contentType,isReview,currentPage);
                });
            }
            else
            {
                PopupController.ShowPopup("popup.cantdeletereview");
            }
        }
        
        private async void ReportReview(string id)
        {
            if (_instance.IsHandlingRequest() || !(await _instance.IsLoggedIn()))
            {
                return;
            }

            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendReportReview(id));
            if (!response.isNetworkError && !response.isHttpError)
            {
                PopupController.ShowPopup("popup.reviewreported");
            }
            else
            {
                switch (response.responseCode)
                {
                    case 409:
                    {
                        PopupController.ShowPopup("popup.reviewalreadyreported");
                        break;
                    }
                    default:
                    {
                        PopupController.ShowPopup("popup.cantreportreview");
                        break;
                    }
                }
            }
        }

        private void SetMaxPage(int maxPage)
        {
            ShopController._instance.reviewPageText.text = $"{currentPage}/{maxPage}";
            this.currentMaxPage = maxPage;
        }

        public void ClickedLeftArrow()
        {
            if (currentPage > 1)
            {
                Show(animationId, contentType, isReview, currentPage - 1);
            }
        }

        public void ClickedRightArrow()
        {
            if (currentPage < currentMaxPage)
            {
                Show(animationId, contentType, isReview, currentPage + 1);
            }
        }

        public async void ClickedRating()
        {
            if (await _instance.IsLoggedIn())
            {
                _instance.addRatingsView.Show(animationId, contentType);
            }
        }

        public override void ClickedBack()
        {
            if (_instance.IsHandlingRequest())
            {
                return;
            }

            if (isReview)
            {
                _instance.uploadCategoryView.Show();
            }
            else
            {
                _instance.browseContentView.ShowWithoutChanging();
            }
        }
    }
}