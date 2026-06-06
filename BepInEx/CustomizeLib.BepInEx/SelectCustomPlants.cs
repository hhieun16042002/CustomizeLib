using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomizeLib.BepInEx
{
    public class SelectCustomPlants : MonoBehaviour
    {
        /// <summary>
        /// 隐藏二创植物界面
        /// </summary>
        public void CloseCustomPlantCards()
        {
            if (CustomPageParent != null)
            {
                CustomPageParent.SetActive(false);
            }
            if (Board.Instance != null && !Board.Instance.boardTag.isIZ)
            {
                InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").GetChild(currentPage).gameObject.SetActive(true);
            }
            else if (Board.Instance != null && Board.Instance.boardTag.isIZ)
            {
                IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid").GetChild(currentPage).gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 初始化二创植物Button
        /// </summary>
        public static void InitCustomCards()
        {
            //控制台支持中文
            Console.OutputEncoding = Encoding.UTF8;
            if (MyShowCustomPlantsButton != null) return;
            //用正常植物Button创建二创植物Button
            if (Board.Instance is not null && !Board.Instance.boardTag.isIZ)
            {
                MyShowCustomPlantsButton = Instantiate(
                    Resources.Load<GameObject>("ui/prefabs/InGameUI").transform.FindChild("Bottom/SeedLibrary/ShowLawn")
                        .gameObject, InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/ShowCardLayout"));
                MyShowCustomPlantsButton.name = "ShowCustom";
                //设置位置
                MyShowCustomPlantsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 72f);
                MyShowCustomPlantsButton.GetComponent<RectTransform>().position = new Vector3(
                    MyShowCustomPlantsButton.GetComponent<RectTransform>().position.x,
                    MyShowCustomPlantsButton.GetComponent<RectTransform>().position.y,
                    InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/ShowLawn").position.z);
                //激活
                MyShowCustomPlantsButton.SetActive(true);

                //摧毁UIButton组件
                if (MyShowCustomPlantsButton.GetComponent<InGameBtn>() != null)
                {
                    Destroy(MyShowCustomPlantsButton.GetComponent<InGameBtn>());
                    MyShowCustomPlantsButton.AddComponent<SelectCustomPlants>();
                }

                //修改文字
                for (int i = 0; i < MyShowCustomPlantsButton.transform.childCount; i++)
                {
                    MyShowCustomPlantsButton.transform.GetChild(i).gameObject.GetComponent<TextMeshProUGUI>().m_text = "二创植物";
                    MyShowCustomPlantsButton.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 初始化IZ下的二创植物Button
        /// </summary>
        public static void InitCustomCards_IZ()
        {
            //控制台支持中文
            Console.OutputEncoding = Encoding.UTF8;
            if (MyShowCustomPlantsButton != null) return;
            //用正常植物Button创建二创植物Button
            if (Board.Instance is not null && Board.Instance.boardTag.isIZ)
            {
                MyShowCustomPlantsButton = Instantiate(
                    IZBottomMenu.Instance.zombieLibary.transform.FindChild("LastPage").gameObject,
                    IZBottomMenu.Instance.plantLibrary.transform);
                MyShowCustomPlantsButton.name = "ShowCustom";
                //设置位置
                MyShowCustomPlantsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-300, 240);
                MyShowCustomPlantsButton.GetComponent<RectTransform>().position = new Vector3(
                    MyShowCustomPlantsButton.GetComponent<RectTransform>().position.x,
                    MyShowCustomPlantsButton.GetComponent<RectTransform>().position.y,
                    IZBottomMenu.Instance.zombieLibary.transform.FindChild("LastPage").position.z);
                //激活
                MyShowCustomPlantsButton.SetActive(true);

                //摧毁UIButton组件
                if (MyShowCustomPlantsButton.GetComponent<UIButton>() != null)
                {
                    Destroy(MyShowCustomPlantsButton.GetComponent<UIButton>());
                    MyShowCustomPlantsButton.AddComponent<SelectCustomPlants>();
                }

                //修改文字
                for (int i = 0; i < MyShowCustomPlantsButton.transform.childCount; i++)
                {
                    MyShowCustomPlantsButton.transform.GetChild(i).gameObject.GetComponent<TextMeshProUGUI>().m_text = "二创植物";
                    MyShowCustomPlantsButton.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 打开二创植物界面
        /// </summary>
        public void OpenCustomPlantCards()
        {
            //基础植物和彩色植物界面隐藏
            if (Board.Instance != null && !Board.Instance.boardTag.isIZ)
            {
                for (int i = 0; i < 5; i++)
                    if (InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").GetChild(i).gameObject.activeSelf)
                    {
                        currentPage = i;
                        InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").GetChild(i).gameObject.SetActive(false);
                        break;
                    }
            }
            else if (Board.Instance != null && Board.Instance.boardTag.isIZ)
            {
                Transform gridTransform = IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid");
                for (int i = 0; i < gridTransform.childCount; i++)
                    if (gridTransform.GetChild(i).gameObject.activeSelf)
                    {
                        currentPage = i;
                        gridTransform.GetChild(i).gameObject.SetActive(false);
                        break;
                    }
            }

            //如果CustomPageParent为空，创建父对象
            if (CustomPageParent == null)
            {
                CustomPageParent = new GameObject("CustomPages");
                CustomPageParent.transform.SetParent(GetParentContainer());
                CustomPageParent.transform.localPosition = Vector3.zero;
                CustomPageParent.transform.localScale = Vector3.one;
            }

            //获取所有二创植物列表
            List<PlantType> plantTypes = GetAllCustomPlants();

            //检查是否需要重新创建页面
            bool needRecreate = false;
            if (MyPageParents == null || MyPageParents.Count == 0)
            {
                needRecreate = true;
            }
            else
            {
                bool allDestroyed = true;
                foreach (var page in MyPageParents)
                {
                    if (page != null)
                    {
                        allDestroyed = false;
                        break;
                    }
                }
                if (allDestroyed)
                {
                    MyPageParents.Clear();
                    needRecreate = true;
                }
            }

            if (needRecreate)
            {
                CreateAllCustomPages(plantTypes);
            }

            // 显示当前页面，隐藏其他页面
            if (MyPageParents != null && MyPageParents.Count > 0)
            {
                // 确保currentCustomPage在有效范围内
                if (currentCustomPage >= MyPageParents.Count)
                {
                    currentCustomPage = 0;
                }

                for (int i = 0; i < MyPageParents.Count; i++)
                {
                    if (MyPageParents[i] != null)
                    {
                        MyPageParents[i].SetActive(i == currentCustomPage);
                    }
                }

                // 显示父对象
                CustomPageParent.SetActive(true);
            }
        }

        /// <summary>
        /// 获取父容器
        /// </summary>
        private Transform GetParentContainer()
        {
            if (Board.Instance != null && !Board.Instance.boardTag.isIZ)
            {
                return InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer");
            }
            else if (Board.Instance != null && Board.Instance.boardTag.isIZ)
            {
                return IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid");
            }
            return null;
        }

        /// <summary>
        /// 获取所有二创植物列表
        /// </summary>
        private List<PlantType> GetAllCustomPlants()
        {
            List<PlantType> plantTypes = new List<PlantType>();
            foreach (PlantType plantType in GameAPP.resourcesManager.allPlants)
            {
                //如果不是融合版植物，就加载
                if (!Enum.IsDefined(typeof(PlantType), plantType) &&
                    PlantDataManager.PlantData_Default.TryGetValue(plantType, out var plantData) && plantData != null)
                {
                    plantTypes.Add(plantType);
                }
            }
            return plantTypes;
        }

        /// <summary>
        /// 创建所有二创植物页面
        /// </summary>
        private void CreateAllCustomPages(List<PlantType> plantTypes)
        {
            // 清理旧的页面
            if (MyPageParents != null)
            {
                foreach (var page in MyPageParents)
                {
                    if (page != null)
                        Destroy(page);
                }
                MyPageParents.Clear();
            }
            else
            {
                MyPageParents = new List<GameObject>();
            }

            //获取模板页面和卡片
            GameObject templatePage = GetTemplatePage();
            GameObject templateCard = GetTemplateCard();

            if (templatePage == null || templateCard == null) return;

            // 如果没有二创植物，创建一个空页面
            if (plantTypes == null || plantTypes.Count == 0)
            {
                // 创建一个空页面
                GameObject emptyPage = CreatePage(templatePage, 0);
                MyPageParents.Add(emptyPage);

                // 获取该页的Grid子对象
                Transform pageGridTransform = emptyPage.transform.GetChild(0);

                // 清空页面上的所有现有卡片
                for (int i = pageGridTransform.childCount - 1; i >= 0; i--)
                {
                    Transform child = pageGridTransform.GetChild(i);
                    if (child.gameObject != templateCard)
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }

                // 隐藏模板卡片
                templateCard.SetActive(false);

                // 初始时隐藏页面
                emptyPage.SetActive(false);

                // 重置页码
                currentCustomPage = 0;
                return;
            }

            int totalPages = Mathf.CeilToInt((float)plantTypes.Count / CardInPage);

            //获取当前卡槽中的植物
            List<PlantType> cardsOnSeedBank = GetCardsOnSeedBank();

            //为每一页创建页面
            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                GameObject newPage = CreatePage(templatePage, pageIndex);
                MyPageParents.Add(newPage);

                //获取该页的Grid子对象（用于放置卡片）
                Transform pageGridTransform = newPage.transform.GetChild(0);

                //清空页面上的所有现有卡片（保留模板卡片）
                for (int i = pageGridTransform.childCount - 1; i >= 0; i--)
                {
                    Transform child = pageGridTransform.GetChild(i);
                    if (child.gameObject != templateCard)
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }

                //计算该页的起始和结束索引
                int startIndex = pageIndex * CardInPage;
                int endIndex = Mathf.Min(startIndex + CardInPage, plantTypes.Count);

                //创建该页的卡片
                for (int i = startIndex; i < endIndex; i++)
                {
                    PlantType plantType = plantTypes[i];
                    GameObject newCard = CreateCard(templateCard, pageGridTransform, plantType, cardsOnSeedBank);
                }

                //隐藏模板卡片
                templateCard.SetActive(false);

                // 初始时所有页面都隐藏
                newPage.SetActive(false);
            }

            //重置页码
            currentCustomPage = 0;
        }

        /// <summary>
        /// 获取模板页面
        /// </summary>
        private GameObject GetTemplatePage()
        {
            if (Board.Instance != null && !Board.Instance.boardTag.isIZ)
            {
                Transform container = InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer");
                return container.FindChild("ColorCards").gameObject;
            }
            else if (Board.Instance != null && Board.Instance.boardTag.isIZ)
            {
                Transform grid = IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid");
                return grid.FindChild("ColorfulCards").gameObject;
            }
            return null;
        }

        /// <summary>
        /// 获取模板卡片
        /// </summary>
        private GameObject GetTemplateCard()
        {
            GameObject templatePage = GetTemplatePage();
            if (templatePage != null)
            {
                Transform pageGrid = templatePage.transform.GetChild(0);
                if (pageGrid.childCount > 0)
                {
                    return pageGrid.GetChild(0).gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// 创建新页面
        /// </summary>
        private GameObject CreatePage(GameObject templatePage, int pageIndex)
        {
            GameObject newPage = Instantiate(templatePage, CustomPageParent.transform);
            newPage.name = $"CustomPage_{pageIndex}";

            // 设置位置和大小与模板相同
            RectTransform rectTransform = newPage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localPosition = Vector3.zero;
                rectTransform.localScale = Vector3.one;
            }

            return newPage;
        }

        /// <summary>
        /// 创建卡片
        /// </summary>
        private GameObject CreateCard(GameObject templateCard, Transform parent, PlantType plantType, List<PlantType> cardsOnSeedBank)
        {
            GameObject newCard = Instantiate(templateCard, parent);
            newCard.SetActive(true);

            //设置位置
            newCard.transform.localPosition = templateCard.transform.localPosition;
            newCard.transform.localScale = templateCard.transform.localScale;
            newCard.transform.localRotation = templateCard.transform.localRotation;

            //背景图片
            Image image = newCard.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            image.sprite = GameAPP.resourcesManager.plantPreviews[plantType].GetComponent<SpriteRenderer>().sprite;
            image.SetNativeSize();

            //设置价格
            newCard.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text =
                PlantDataManager.PlantData_Default[plantType].cost.ToString();

            //卡片组件
            CardUI component = newCard.transform.GetChild(1).GetComponent<CardUI>();
            component.gameObject.SetActive(true);

            //修改图片
            Mouse.Instance.ChangeCardSprite(plantType, component);

            //修改缩放
            newCard.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = true;
            RectTransform bgRect = newCard.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
            RectTransform packetRect = newCard.transform.GetChild(1).GetChild(0).GetComponent<RectTransform>();
            bgRect.localScale = packetRect.localScale;
            bgRect.sizeDelta = packetRect.sizeDelta;

            //设置数据
            component.thePlantType = plantType;
            component.theSeedType = (int)plantType;
            component.theSeedCost = PlantDataManager.PlantData_Default[plantType].cost;
            component.fullCD = PlantDataManager.PlantData_Default[plantType].cd;

            if (Board.Instance != null && Board.Instance.boardTag.isIZ)
            {
                component.theSeedCost = 0;
                component.fullCD = 0f;
            }

            if (cardsOnSeedBank.Contains(plantType))
                newCard.transform.GetChild(1).gameObject.SetActive(false);

            CheckCardState customComponent = newCard.AddComponent<CheckCardState>();
            customComponent.card = newCard;
            customComponent.cardType = component.thePlantType;

            return newCard;
        }

        /// <summary>
        /// 获取当前卡槽中的植物
        /// </summary>
        private List<PlantType> GetCardsOnSeedBank()
        {
            List<PlantType> cardsOnSeedBank = new List<PlantType>();

            if (Board.Instance != null && !Board.Instance.boardTag.isIZ)
            {
                GameObject seedGroup = InGameUI.Instance.SeedBank.transform.GetChild(0).gameObject;
                for (int i = 0; i < seedGroup.transform.childCount; i++)
                {
                    GameObject seed = seedGroup.transform.GetChild(i).gameObject;
                    if (seed.transform.childCount > 0)
                        cardsOnSeedBank.Add(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType);
                }
            }
            else if (Board.Instance != null && Board.Instance.boardTag.isIZ)
            {
                GameObject seedGroup = InGameUI_IZ.Instance.transform.GetChild(0).GetChild(0).gameObject;
                for (int i = 0; i < seedGroup.transform.childCount; i++)
                {
                    GameObject seed = seedGroup.transform.GetChild(i).gameObject;
                    if (seed.transform.childCount > 0)
                        cardsOnSeedBank.Add(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType);
                }
            }

            return cardsOnSeedBank;
        }

        /// <summary>
        /// 切换到下一页
        /// </summary>
        public void NextPage()
        {
            if (MyPageParents == null || MyPageParents.Count == 0) return;

            // 检查当前是否是最后一页
            bool isLastPage = (currentCustomPage == MyPageParents.Count - 1);

            if (isLastPage)
            {
                // 如果是最后一页，关闭二创植物界面
                CloseCustomPlantCards();
            }
            else
            {
                // 隐藏当前页
                if (currentCustomPage >= 0 && currentCustomPage < MyPageParents.Count && MyPageParents[currentCustomPage] != null)
                {
                    MyPageParents[currentCustomPage].SetActive(false);
                }

                // 切换到下一页
                currentCustomPage++;

                // 显示新页面
                if (currentCustomPage >= 0 && currentCustomPage < MyPageParents.Count && MyPageParents[currentCustomPage] != null)
                {
                    MyPageParents[currentCustomPage].SetActive(true);
                }
            }
        }

        /// <summary>
        /// 切换到上一页
        /// </summary>
        public void PreviousPage()
        {
            if (MyPageParents == null || MyPageParents.Count == 0) return;

            // 检查当前是否是第一页
            bool isFirstPage = (currentCustomPage == 0);

            if (isFirstPage)
            {
                // 如果是第一页，关闭二创植物界面
                CloseCustomPlantCards();
            }
            else
            {
                // 隐藏当前页
                if (currentCustomPage >= 0 && currentCustomPage < MyPageParents.Count && MyPageParents[currentCustomPage] != null)
                {
                    MyPageParents[currentCustomPage].SetActive(false);
                }

                // 切换到上一页
                currentCustomPage--;

                // 显示新页面
                if (currentCustomPage >= 0 && currentCustomPage < MyPageParents.Count && MyPageParents[currentCustomPage] != null)
                {
                    MyPageParents[currentCustomPage].SetActive(true);
                }
            }
        }

        /// <summary>
        /// 类似Update
        /// </summary>
        public void Update()
        {
            //判断鼠标按下
            if (Input.GetMouseButtonDown(0) && MyShowCustomPlantsButton != null)
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

                //击中二创植物Button
                if (hit.collider != null && hit.collider.gameObject == MyShowCustomPlantsButton && hit.collider.gameObject.name == "ShowCustom")
                {
                    if (CustomPageParent != null && CustomPageParent.activeSelf)
                    {
                        // 如果已经在二创植物界面，优先跳到下一页（如果有下一页）
                        if (currentCustomPage < MyPageParents.Count - 1)
                        {
                            NextPage();
                        }
                        else
                        {
                            // 已经是最后一页，关闭界面
                            CloseCustomPlantCards();
                        }
                    }
                    else
                    {
                        // 打开二创植物页面，重置到第一页
                        currentCustomPage = 0;
                        OpenCustomPlantCards();
                    }
                }
            }

            // 处理翻页快捷键
            if (CustomPageParent != null && CustomPageParent.activeSelf)
            {
                // 按右键或D键切换到下一页
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    NextPage();
                }
                // 按左键或A键切换到上一页
                else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    PreviousPage();
                }
            }

            //设置鼠标特效
            if (MyShowCustomPlantsButton != null)
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == MyShowCustomPlantsButton)
                {
                    CursorChange.SetClickCursor();
                }
            }
        }

        public static int CardInPage => 6 * 9;

        /// <summary>
        /// 二创植物页面列表（支持多页）
        /// </summary>
        public static List<GameObject>? MyPageParents { get; set; }

        /// <summary>
        /// 二创植物Button
        /// </summary>
        public static GameObject? MyShowCustomPlantsButton { get; set; }

        /// <summary>
        /// 二创植物页面的父对象
        /// </summary>
        public static GameObject? CustomPageParent { get; set; }

        /// <summary>
        /// 当前显示的二创植物页码
        /// </summary>
        public int currentCustomPage = 0;

        /// <summary>
        /// 当前基础植物页面索引
        /// </summary>
        public int currentPage = 0;
    }
}