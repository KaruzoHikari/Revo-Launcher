using System.Collections.Generic;
using System.Globalization;
using Animations;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ShopViews
{
    public class AddRatingShopView : ShopView
    {
        public string existingReviewId;
        private string animationId;
        private int rating = 3;
        private ContentType contentType;
        private Dictionary<int, Image> stars = new Dictionary<int, Image>();

        public AddRatingShopView(GameObject view) : base(view, () => { })
        {
            SpawnStars();
        }

        private void SpawnStars()
        {
            Transform starContent = view.transform.Find("Content").Find("Stars");
            for (int i = 1; i <= 5; i++)
            {
                GameObject star = GameObject.Instantiate(_instance.starPrefab, starContent);
                int number = i;
                star.GetComponent<Button>().onClick.AddListener(() => SetNewRating(number));
                stars[i] = star.GetComponent<Image>();
            }
        }

        private void SetNewRating(int rating)
        {
            for (int i = 1; i <= rating; i++)
            {
                stars[i].color = ThemeController.GetColor(ThemeColor.HubStarIcon);
            }
            for (int i = rating + 1; i <= 5; i++)
            {
                stars[i].color = ThemeController.GetColor(ThemeColor.HubUnstarIcon);
            }
            this.rating = rating;
        }

        public async void Show(string id, ContentType contentType)
        {
            base.Show();
            this.contentType = contentType;
            this.animationId = id;
            _instance.deleteReviewButton.SetActive(false);

            // We need to check if you already uploaded a review, in that case we update the data
            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendGetReview(contentType, id, ShopController.GetUserId()));
            if (!response.isNetworkError && !response.isHttpError)
            {
                // We load in the review
                JObject jObject = response.GetJsonResponse();
                _instance.reviewDescription.text = jObject["description"]?.ToString();
                existingReviewId = jObject["id"]?.ToString();
                _instance.deleteReviewButton.SetActive(true);
                SetNewRating((int)jObject["rating"]);
                return;
            }
            
            // Otherwise we just create a new review
            _instance.reviewDescription.text = "";
            existingReviewId = null;
            SetNewRating(3);
        }

        public async void ClickedSendReview()
        {
            if (_instance.IsHandlingRequest())
            {
                return;
            }

            string description = _instance.reviewDescription.text.Trim();
            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendAddReview(contentType, animationId, description, rating));
            if (!response.isNetworkError && !response.isHttpError)
            {
                PopupController.ShowPopup("popup.sentreview", ClickedBack);
            }
            else
            {
                switch (response.responseCode)
                {
                    case 409:
                    {
                        PopupController.ShowPopup("popup.cantreview", ClickedBack);
                        break;
                    }
                }
            }
        }
        
        public void ClickedDeleteReview()
        {
            if (_instance.IsHandlingRequest() || existingReviewId == null)
            {
                return;
            }
            
            PopupController.ShowPopup("popup.deletereview", DeleteReview, PopupController.ClosePopup);
        }

        private async void DeleteReview()
        {
            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendDeleteReview(existingReviewId));
            if (!response.isNetworkError && !response.isHttpError)
            {
                PopupController.ShowPopup("popup.reviewdeleted", ClickedBack);
            }
            else
            {
                PopupController.ShowPopup("popup.cantdeletereview");
            }
        }

        public override void ClickedBack()
        {
            if (!_instance.IsHandlingRequest())
            {
                _instance.ratingsView.Show(animationId, contentType);
            }
        }
    }
}