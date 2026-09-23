using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Object = System.Object;

namespace Misc
{
    public static class OptionsSpawner
    {
        public static void SetupOptions(HasOptions options, RectTransform parent)
        {
            // Then we create the new ones
            List<Option> optionList = options.GetOptions();

            foreach (Option option in optionList)
            {
                SetupOption(option, parent);
            }
        }

        public static GameObject SetupOption(Option option, RectTransform parent, GameObject specialPrefab = null)
        {
            Type optionType = option.type;
            if (optionType == typeof(int))
            {
                return SummonOption_Int((Option<int>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(float))
            {
                return SummonOption_Float((Option<float>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(Vector2))
            {
                return SummonOption_Vector2((Option<Vector2>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(Vector3))
            {
                return SummonOption_Vector3((Option<Vector3>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(Color))
            {
                return SummonOption_Color((Option<Color>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(Ease))
            {
                return SummonOption_Enum((Option<Ease>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(SystemLanguage))
            {
                return SummonOption_Enum((Option<SystemLanguage>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(PathType))
            {
                return SummonOption_Enum((Option<PathType>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(WebRequestController.RequestType))
            {
                return SummonOption_Enum((Option<WebRequestController.RequestType>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(bool))
            {
                return SummonOption_Boolean((Option<bool>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(string))
            {
                return SummonOption_String((Option<string>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(RotationAnimation.RotationMode))
            {
                return SummonOption_Enum((Option<RotationAnimation.RotationMode>) option, parent, specialPrefab);
            }
            else if (optionType == typeof(List<Vector2>))
            {
                return SummonOption_List((Option<List<Vector2>>) option, parent);
            }

            return null;
        }

        public static GameObject SummonVoid(RectTransform rectTransform, float height)
        {
            GameObject spawned = new GameObject("Void_" + height, typeof(RectTransform));
            spawned.transform.SetParent(rectTransform);
            spawned.GetComponent<RectTransform>().sizeDelta = new Vector2(100, height);
            return spawned;
        }
        
        public static GameObject SummonTitle(RectTransform rectTransform, string text, bool adjustHeight = true)
        {
            return SummonTextPrefab(EditorController._instance.titlePrefab, rectTransform, text, adjustHeight);
        }

        private static GameObject SummonTextPrefab(GameObject prefab, RectTransform rectTransform, string text, bool adjustHeight = true)
        {
            GameObject spawned = GameObject.Instantiate(prefab, rectTransform);
            string title = TextController.GetTranslation(text);
            spawned.name = text;
            TextMeshProUGUI textMesh = spawned.transform.Find("Title").gameObject.GetComponent<TextMeshProUGUI>();

            float currentSize = spawned.GetComponent<RectTransform>().sizeDelta.y;
            textMesh.text = title;
            float preferredHeight = adjustHeight ? textMesh.preferredHeight : Math.Max(textMesh.preferredHeight + currentSize/2, currentSize);
            spawned.GetComponent<RectTransform>().sizeDelta = new Vector2(935, preferredHeight);
            return spawned;
        }
        
        public static GameObject SummonOption_Boolean(Option<bool> option, RectTransform rectTransform, GameObject booleanPrefab = null)
        {
            GameObject prefab = booleanPrefab ?? EditorController._instance.booleanPrefab;
            bool value = option.getter();

            GameObject spawnedOption = SpawnPrefab(prefab, option, rectTransform);
            StaticUtils.ChangeBooleanImage(spawnedOption, value);

            spawnedOption.transform.Find("Elements").Find("ImageHolder").GetComponent<Button>().onClick.AddListener(() =>
            {
                option.setter(!option.getter());
                StaticUtils.ChangeBooleanImage(spawnedOption, option.getter());
            });
            return spawnedOption;
        }

        public static GameObject SummonOption_Int(Option<int> option, RectTransform rectTransform, GameObject vectorPrefab = null)
        {
            int value = option.getter();
            GameObject prefab = vectorPrefab ?? EditorController._instance.vectorPrefab;
            GameObject spawnedOption = SpawnPrefab(prefab, option, rectTransform);

            TMP_InputField inputField = spawnedOption.GetComponentInChildren<TMP_InputField>();
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.text = value.ToString();
            inputField.onValueChanged.AddListener(delegate
            {
                try
                {
                    option.setter(int.Parse(inputField.text));
                }
                catch (Exception e)
                {
                    // ignored
                }
            });
            TabController.AddInputField(inputField);
            return spawnedOption;
        }

        public static GameObject SummonOption_Float(Option<float> option, RectTransform rectTransform, GameObject vectorPrefab = null)
        {
            float value = option.getter();
            GameObject prefab = vectorPrefab ?? EditorController._instance.vectorPrefab;
            GameObject spawnedOption = SpawnPrefab(prefab, option, rectTransform);
            TMP_InputField inputField = spawnedOption.GetComponentInChildren<TMP_InputField>();
            inputField.contentType = PREFS.DecimalAsString.GetBool() ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.DecimalNumber;
            inputField.text = value.ToString();
            // todo kaz change to onValueChanged?
            inputField.onValueChanged.AddListener(delegate
            {
                try
                {
                    option.setter(float.Parse(inputField.text));
                }
                catch (Exception e)
                {
                    // ignored
                }
            });
            TabController.AddInputField(inputField);
            return spawnedOption;
        }

        public static GameObject SummonOption_String(Option<string> option, RectTransform rectTransform, GameObject stringPrefab = null)
        {
            string value = option.getter();
            GameObject prefab = stringPrefab ?? EditorController._instance.stringPrefab;
            GameObject spawnedOption = SpawnPrefab(prefab, option, rectTransform);
            TMP_InputField inputField = spawnedOption.GetComponentInChildren<TMP_InputField>();
            inputField.contentType = TMP_InputField.ContentType.Standard;
            inputField.text = value;
            inputField.onValueChanged.AddListener(delegate { option.setter(inputField.text); });
            TabController.AddInputField(inputField);
            return spawnedOption;
        }

        public static GameObject SummonOption_Color(Option<Color> option, RectTransform rectTransform, GameObject colorPrefab = null)
        {
            Color value = option.getter();

            GameObject prefab = colorPrefab ?? EditorController._instance.colorPrefab;
            GameObject spawnedOption = SpawnPrefab(prefab, option, rectTransform);
            GameObject color = spawnedOption.transform.Find("ColorHolder").Find("Color").gameObject;
            UICornerCut cornerCut = color.GetComponent<UICornerCut>();
            cornerCut.color = value;
            color.GetComponent<Button>().onClick.AddListener(() =>
            {
                ColorPickerController._instance.ShowColorPicker(option.getter(), newColor =>
                {
                    cornerCut.color = newColor;
                    option.setter(newColor);
                });
            });
            return spawnedOption;
        }

        public static GameObject SummonOption_List<T>(Option<List<T>> option, RectTransform rectTransform, RectTransform rootTransform = null,
            GameObject foldoutPrefab = null)
        {
            List<T> value = option.getter();
            GameObject prefab = foldoutPrefab ?? EditorController._instance.foldoutPrefab;

            GameObject spawnedOption = SpawnPrefab(prefab, option, rectTransform);
            RectTransform foldout = spawnedOption.transform.Find("Foldout").GetComponent<RectTransform>();
            for (int i = 0; i < value.Count; i++)
            {
                int index = i;
                Option<T> listOption = Option.Create(typeof(T).Name, () => value[index], x => value[index] = x);
                SetupOption(listOption, foldout);
            }

            RectTransform parent = rootTransform == null ? EditorController._instance.optionsHolder : rootTransform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);

            spawnedOption.transform.Find("TitleHolder").Find("Arrow").GetComponent<Button>().onClick.AddListener(() =>
            {
                foldout.gameObject.SetActive(!foldout.gameObject.activeInHierarchy);
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            });

            spawnedOption.transform.Find("TitleHolder").Find("Add").GetComponent<Button>().onClick.AddListener(() =>
            {
                foldout.gameObject.SetActive(true);
                T newVector = (T) Activator.CreateInstance(typeof(T));
                value.Add(newVector);
                int index = value.Count - 1;
                Option<T> listOption = Option.Create(typeof(T).Name, () => value[index], x => value[index] = x);
                SetupOption(listOption, foldout);
                LayoutRebuilder.ForceRebuildLayoutImmediate(foldout);
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            });

            spawnedOption.transform.Find("TitleHolder").Find("Remove").GetComponent<Button>().onClick.AddListener(() =>
            {
                value.RemoveAt(value.Count - 1);
                GameObject.Destroy(foldout.GetChild(foldout.childCount - 1).gameObject);
                LayoutRebuilder.ForceRebuildLayoutImmediate(foldout);
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            });

            return spawnedOption;
        }

        public static GameObject SummonOption_Vector2(Option<Vector2> option, RectTransform rectTransform, GameObject vectorPrefab = null)
        {
            Vector2 value = option.getter();
            Dictionary<int, TMP_InputField> fields = new Dictionary<int, TMP_InputField>();

            return HandleVectorSpawn(option, value, 2, rectTransform, fields, delegate
            {
                try
                {
                    float x = float.Parse(fields[0].text);
                    float y = float.Parse(fields[1].text);
                    option.setter(new Vector2(x, y));
                }
                catch (Exception e)
                {
                    // ignored
                }
            }, vectorPrefab ?? EditorController._instance.vectorPrefab);
        }

        public static GameObject SummonOption_Vector3(Option<Vector3> option, RectTransform rectTransform, GameObject vectorPrefab = null)
        {
            Vector3 value = option.getter();
            Dictionary<int, TMP_InputField> fields = new Dictionary<int, TMP_InputField>();

            return HandleVectorSpawn(option, value, 3, rectTransform, fields, delegate
            {
                try
                {
                    float x = float.Parse(fields[0].text);
                    float y = float.Parse(fields[1].text);
                    float z = float.Parse(fields[2].text);
                    option.setter(new Vector3(x, y, z));
                }
                catch (Exception e)
                {
                    // ignored
                }
            }, vectorPrefab ?? EditorController._instance.vectorPrefab);
        }

        private static GameObject HandleVectorSpawn(Option option, Object value, int vectorSize, RectTransform parent,
            Dictionary<int, TMP_InputField> fields, UnityAction<string> call, GameObject vectorPrefab)
        {
            GameObject spawnedOption = SpawnPrefab(vectorPrefab, option, parent);

            TMP_InputField xField = spawnedOption.GetComponentInChildren<TMP_InputField>();
            fields.Add(0, xField);

            for (int i = 1; i < vectorSize; i++)
            {
                GameObject clone = GameObject.Instantiate(xField.gameObject, xField.transform.parent);
                fields.Add(i, clone.GetComponent<TMP_InputField>());
            }

            foreach (int number in fields.Keys)
            {
                TMP_InputField inputField = fields[number];
                inputField.contentType = PREFS.DecimalAsString.GetBool() ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.DecimalNumber;
                if (vectorSize == 2)
                {
                    inputField.text = number == 0 ? ((Vector2) value).x.ToString() : ((Vector2) value).y.ToString();
                }
                else if (vectorSize > 2)
                {
                    inputField.text = number == 0 ? ((Vector3) value).x.ToString() :
                        number == 1 ? ((Vector3) value).y.ToString() : ((Vector3) value).z.ToString();
                }

                inputField.onValueChanged.AddListener(call);
                TabController.AddInputField(inputField);
            }

            return spawnedOption;
        }

        public static GameObject SummonOption_Enum<T>(Option<T> option, RectTransform parent, GameObject dropdownPrefab = null)
        {
            List<string> valueList = new List<string>();
            foreach (Object value in Enum.GetValues(option.type))
            {
                valueList.Add(value.ToString());
            }
            FilterKnownValueTypes<T>(valueList);
            return SummonOption_Dropdown(option, parent, valueList, text =>
            {
                option.setter((T)Enum.Parse(option.type, text));
            }, dropdownPrefab);
        }

        public static GameObject SummonOption_DropdownString(Option<string> option, RectTransform parent, List<string> valueList, GameObject dropdownPrefab = null)
        {
            return SummonOption_Dropdown(option, parent, valueList, text =>
            {
                option.setter(text);
            }, dropdownPrefab);
        }

        public static GameObject SummonOption_Dropdown<T>(Option<T> option, RectTransform parent, List<string> valueList, UnityAction<string> setter, GameObject dropdownPrefab = null)
        {
            GameObject prefab = dropdownPrefab ?? EditorController._instance.dropdownPrefab;
            GameObject spawnedOption = SpawnPrefab(prefab, option, parent);
            TMP_Dropdown dropdown = spawnedOption.transform.Find("Dropdown").gameObject.GetComponent<TMP_Dropdown>();

            // We clear all the default dropdown values
            dropdown.ClearOptions();

            // We add all the Eases to the dropdown
            dropdown.AddOptions(valueList);

            // We set the default to the current ease
            int foundValue = 0;
            string currentName = option.getter().ToString();
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                TMP_Dropdown.OptionData data = dropdown.options[i];
                if (data.text.Equals(currentName))
                {
                    foundValue = i;
                    break;
                }
            }

            dropdown.SetValueWithoutNotify(foundValue);

            // We assign the setter
            dropdown.onValueChanged.AddListener(index =>
            {
                string text = dropdown.options[index].text;
                setter.Invoke(text);
            });

            return spawnedOption;
        }

        private static void FilterKnownValueTypes<T>(List<string> valueList)
        {
            List<string> removeList = new List<string>();

            // here we're gonna filter the languages since we only want those that are currently available in the app when used
            if (typeof(T) == typeof(SystemLanguage))
            {
                foreach (string value in valueList)
                {
                    if (!TextController._instance.availableLanguages.ContainsKey(value))
                    {
                        removeList.Add(value);
                    }
                }
            }
            
            removeList.ForEach(x => valueList.Remove(x));
        }
        
        public static GameObject SpawnPrefab(GameObject prefab, Option option, RectTransform parent)
        {
            return SpawnPrefab(prefab, option.tag, option.info, parent);
        }

        public static GameObject SpawnPrefab(GameObject prefab, string id, string description, RectTransform parent)
        {
            string title = TextController.GetTranslation(id);
            GameObject spawned = GameObject.Instantiate(prefab, parent);
            spawned.name = title;

            GameObject titleHolder = spawned.transform.Find("TitleHolder").gameObject;
            titleHolder.transform.Find("Title").gameObject.GetComponent<TextMeshProUGUI>().text = title;
            if (!string.IsNullOrEmpty(description))
            {
                titleHolder.GetComponent<Button>().onClick.AddListener(() => PopupController.ShowPopup(description));
            }

            spawned.SetLayerAllChildren(parent.gameObject.layer);
            return spawned;
        }
    }
}