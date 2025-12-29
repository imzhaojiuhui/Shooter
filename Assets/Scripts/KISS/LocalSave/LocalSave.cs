using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KISS
{
    public class LocalSave : Singleton<LocalSave>
    {
        /// <summary>
        /// 不支持HashSet
        /// </summary>
        [Serializable]
        private class Data
        {
            public JObject mSaveObject = new JObject();
            public Dictionary<string, DateTime> TodayMark = new Dictionary<string, DateTime>();
        }

        private Data _data;
        private long _userID;
        private string FileName => $"LocalSave_{_userID}.json";

        public void Load(long userID)
        {
            _userID = userID;
            _data = SerializeHelper.DeserializeJson<Data>(FileName);
            if (_data == null)
            {
                _data = new Data();
            }

            // _data.TodayMark ??= new();
        }

        public void SetKeyData<T>(string key, T data)
        {
            JToken fromObject = JToken.FromObject(data);
            _data.mSaveObject[key] = fromObject;
            _isDirty = true;
        }

        public void Save<T>(string key, T data)
        {
            SetKeyData<T>(key, data);
            Save();
        }

        public T Get<T>(string key, T defaultValue=default)
        {
            if (_data.mSaveObject.TryGetValue(key, out var value))
            {
                return value.Value<T>();
            }

            return defaultValue;
        }
        
        public List<T> GetList<T>(string key)
        {
            if (_data.mSaveObject.TryGetValue(key, out var value))
            {
                var jArray = value as JArray;
                if (jArray != null)
                    return jArray.ToObject<List<T>>();
                // return value.Value<T>();
            }

            return null;
        }

        private bool _isDirty;

        public void MarkDirty()
        {
            _isDirty = true;
        }

        public void SaveIfDirty()
        {
            if (!_isDirty)
            {
                return;
            }

            Save();
            _isDirty = false;
        }

        private void Save()
        {
            SerializeHelper.SerializeJson(FileName, _data);
        }
        
        // public bool HaveTodayMark(string key)
        // {
        //     var markDate = _data.TodayMark.GetValueOrDefault(key, DateTime.MinValue).Date;
        //     return markDate == Utils.ServerNow.Date;
        // }
        //
        // public void SetTodayMark(string key, bool value = true)
        // {
        //     if (value == HaveTodayMark(key))
        //     {
        //         return;
        //     }
        //
        //     var markDate = value ? Utils.ServerNow.Date : DateTime.MinValue;
        //     _data.TodayMark[key] = markDate;
        //     _isDirty = true;
        // }
    }
}