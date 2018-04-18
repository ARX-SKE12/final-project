﻿
namespace ARTag {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class TextTagBehaviour : TagBehaviour
    {

        public GameObject description;

        protected override JSONObject PrepareTagData(Dictionary<string, object> data)
        {
            JSONObject jsonData = base.PrepareTagData(data);
            JSONObject detail = jsonData.GetField("detail");
            detail.AddField("description", ((string) data["description"]));
            jsonData.SetField("detail", detail);
            Debug.Log(((string)data["description"]).IndexOf("\n"));
            return jsonData;
        }

        protected override void ConstructTag(JSONObject data)
        {
            base.ConstructTag(data);
            description.GetComponent<Text>().text = data.GetField("tag").GetField("detail").GetField("description").str;
        }
    }

}