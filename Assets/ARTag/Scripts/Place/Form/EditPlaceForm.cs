﻿
namespace ARTag
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;
    using TMPro;
    using SocketIOManager;
    using SocketIO;

    public class EditPlaceForm : MonoBehaviour
    {

        SocketManager socketManager;
        TemporaryDataManager tempManager;

        const string FIELD_UPDATE_DATA = "updatedData";
        const string FIELD_ID = "id";
        const string FIELD_NAME = "name";
        const string FIELD_DESCRIPTOPN = "description";
        const string FIELD_THUMBNAIL = "thumbnail";
        const string FIELD_IS_PUBLIC = "isPublic";
        const string FIELD_WIDTH = "width";
        const string FIELD_HEIGHT = "height";
        const string FIELD_IMAGE = "image";
        const string BASE_URL = "https://storage.googleapis.com/artag-thumbnail/";

        public GameObject placeNameText, placeDescriptionText, publicToggle, thumbnailPicker, creatingPanel, preloader, errorNotification, submitButton;

        bool isUpdate;

        void Start()
        {
            socketManager = GameObject.Find(ObjectsCollector.SOCKETIO_MANAGER_OBJECT).GetComponent<SocketManager>();
            tempManager = GameObject.FindObjectOfType<TemporaryDataManager>();
            socketManager.On(EventsCollector.PLACE_CREATE_SUCCESS, OnPlaceCreateSuccess);
            socketManager.On(EventsCollector.PLACE_CREATE_ERROR, OnPlaceCreateError);
            socketManager.On(EventsCollector.PLACE_UPDATE_SUCCESS, OnPlaceCreateSuccess);
            socketManager.On(EventsCollector.PLACE_UPDATE_ERROR, OnPlaceCreateError);
            if (tempManager.Has("currentPlace"))
            {
                creatingPanel.GetComponentInChildren<Text>().text = "Updating";
                submitButton.GetComponentInChildren<Text>().text = "Update";
                Place place = (Place)tempManager.Get("currentPlace");
                isUpdate = true;
                thumbnailPicker.GetComponent<ImagePicker>().SetValue(BASE_URL + place.timestamp + "-" + place.name + ".png");
                placeNameText.GetComponent<InputField>().text = place.name;
                placeDescriptionText.GetComponent<TMP_InputField>().text = place.description;
                publicToggle.GetComponent<Toggle>().isOn = place.isPublic;
            }
        }

        #region Form
        public void SubmitForm()
        {
            StartCoroutine(CreatePlace());
        }

        IEnumerator CreatePlace()
        {
            List<string> missing = new List<string>();
            string name = placeNameText.GetComponent<InputField>().text;
            if (name == null || name == "") missing.Add("Place Name");
            string description = placeDescriptionText.GetComponent<TMP_InputField>().text;
            bool isPublic = publicToggle.GetComponent<Toggle>().isOn;
            ImageData thumbnail = thumbnailPicker.GetComponent<ImagePicker>().selectedImage;
            if (thumbnail == null) missing.Add("Thumbnail");
            if (missing.Count > 0)
            {
                string error = missing[0];
                for (int i = 1; i < missing.Count; i++) error += ", " + missing[i];
                error += " are required fields";
                errorNotification.GetComponentInChildren<Text>().text = error;
                errorNotification.SetActive(true);
            } else
            {
                creatingPanel.SetActive(true);
                yield return new WaitUntil(() => creatingPanel.activeSelf);
                JSONObject data = new JSONObject();
                data.AddField(FIELD_NAME, name);
                data.AddField(FIELD_DESCRIPTOPN, description);
                data.AddField(FIELD_IS_PUBLIC, isPublic);
                JSONObject thumbnailData = new JSONObject();
                thumbnailData.SetField(FIELD_WIDTH, thumbnail.width);
                thumbnailData.SetField(FIELD_HEIGHT, thumbnail.height);
                thumbnailData.AddField(FIELD_IMAGE, thumbnail.data);
                data.AddField(FIELD_THUMBNAIL, thumbnailData);
                if (!isUpdate) socketManager.Emit(EventsCollector.PLACE_CREATE, data);
                else
                {
                    JSONObject toSendData = new JSONObject();
                    toSendData.AddField(FIELD_ID, ((Place)tempManager.Get("currentPlace")).id);
                    toSendData.AddField(FIELD_UPDATE_DATA, data);
                    socketManager.Emit(EventsCollector.PLACE_UPDATE, toSendData);
                }
            }
        }

        public void OnPlaceCreateSuccess(SocketIOEvent e)
        {
            string timestamp = e.data.GetField("place").GetField("timestamp").str;
            string name = e.data.GetField("place").GetField("name").str;
            string significant = timestamp + "-" + name;
            Place place = new Place(e.data.GetField("place"));
            tempManager.Put("significant", significant);
            tempManager.Put("currentPlace", place);
            if (isUpdate) creatingPanel.GetComponentInChildren<Text>().text = "Place Update Success!";
            else creatingPanel.GetComponentInChildren<Text>().text = "Place Creation Success!";
            preloader.SetActive(false);
            SceneManager.LoadScene("QR Loader");
        }

        public void OnPlaceCreateError(SocketIOEvent e)
        {
            creatingPanel.SetActive(false);
            errorNotification.GetComponentInChildren<Text>().text = e.data.GetField("error").str;
            errorNotification.SetActive(true);
        }
#endregion
    }
}
