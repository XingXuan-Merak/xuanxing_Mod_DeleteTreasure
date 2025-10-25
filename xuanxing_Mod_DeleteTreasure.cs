using Mods;
using Tool.Database;
using UnityEngine;

namespace xuanxing_Mod_DeleteTreasure
{
    public class LoadMod : UserMod
    {
        public override void OnLoad()
        {
            base.OnLoad();
            new GameObject("Delete").AddComponent<DeleteTreasure>();
            UnityEngine.Debug.Log("【宝物移除mod】执行：OnLoad");
        }
    }
    class DeleteTreasure : MonoBehaviour
    {
        //选定框组件
        //宝物单元格
        //位置索引
        //初始关闭
        public Transform? frame;
        private TreasureCell? target;
        private int currentIndex = 0;
        private bool FrameIsActive = false;
        private void Awake()
        {
            frame = new GameObject("SelectionFrame").transform;
            SpriteRenderer renderer = frame.gameObject.AddComponent<SpriteRenderer>();
            Sprite sprite = Resources.Load<Sprite>("sprites/icon/common/clear_record_frame");
            if (sprite == null) Debug.LogError("【宝物移除mod】错误：未找到图片资源");
            renderer.sprite = sprite;
            renderer.sortingLayerName = "UI";
            renderer.sortingOrder = 99;
            frame.gameObject.SetActive(false);
            UnityEngine.Debug.Log("【宝物移除mod】执行：Awake");
        }
        private void OnDestroy()
        {
            if (frame != null)
            {
                Destroy(frame.gameObject);
                frame = null;
            }
        }
        //若选定框未激活时return
        //若宝物栏无宝物时关闭选定框
        //选定框更新至当前单元格
        private void LateUpdate()
        {
            if (frame == null || frame.gameObject == null || !frame.gameObject.activeSelf) return;
            if (this.target == null || !this.target.gameObject.activeInHierarchy)
            {
                this.frame.gameObject.SetActive(false);
                FrameIsActive = false;
                return;
            }
            this.frame.transform.position = this.target.transform.position;
        }
        //选定框更新至当前单元格
        private void UpdateFrameTarget()
        {
            if (!FrameIsActive || frame == null || frame.gameObject == null) return;
            var itemManager = Singleton<ItemManager>.Instance();
            if (itemManager?.treasures == null) return;
            var treasures = itemManager.treasures;
            if (currentIndex < 0 || currentIndex >= treasures.Count)
            {
                currentIndex = 0;
                Toggle();
                return;
            }
            this.target = treasures[currentIndex].treasureCell;
            if (this.target == null)
            {
                frame.gameObject.SetActive(false);
                FrameIsActive = false;
                return;
            }
            frame.transform.position = this.target.transform.position;
        }
        //键盘输入
        //若未激活时return
        private void Update()
        {
            HandleKeyboardInput();
        }
        private void HandleKeyboardInput()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Equals) || UnityEngine.Input.GetKeyDown(KeyCode.KeypadEquals)) Toggle();
            if (!FrameIsActive) return;
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow)) Move(-1);
            if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow)) Move(1);
            if (UnityEngine.Input.GetKeyDown(KeyCode.Backspace)) Delete();
        }
        //“=”键切换状态
        //重新开启时选定框回至首位
        private void Toggle()
        {
            FrameIsActive = !FrameIsActive;
            if (frame != null && frame.gameObject != null) frame.gameObject.SetActive(FrameIsActive);
            if (FrameIsActive)
            {
                currentIndex = 0;
                UpdateFrameTarget();
            }
        }
        //“←”“→”键移动选定框
        //若宝物计数为0切换状态
        //选定框更新至当前单元格
        private void Move(int direction)
        {
            int treasureCount = Singleton<ItemManager>.Instance().treasures.Count;
            if (treasureCount == 0)
            {
                Toggle();
                return;
            }
            currentIndex = (currentIndex + direction + treasureCount) % treasureCount;
            UpdateFrameTarget();
        }
        //退格键移除选定框处宝物
        //若宝物计数为0切换状态
        //选定框更新至当前单元格
        private void Delete()
        {
            var itemManager = Singleton<ItemManager>.Instance();
            if (itemManager == null || itemManager.treasures == null) return;
            var treasures = itemManager.treasures;
            if (currentIndex < 0 || currentIndex >= treasures.Count) return;
            ItemTreasure Remove = treasures[currentIndex];
            Singleton<ItemManager>.Instance().RemoveTreasure(Remove);
            Singleton<EventCenter>.Instance().EventTrigger(EventName.refreshTreasure);
            DataTreasure dataTreasure = Remove.dataTreasure;
            UnityEngine.Debug.Log($"【宝物移除mod】执行：Remove{dataTreasure.Name}");
            if (dataTreasure.InGame && (dataTreasure.GainType == 0 || dataTreasure.GainType == 1))
            {
                Singleton<WantedManager>.Instance().wantedProcess.AddTreasurePool(dataTreasure);
                UnityEngine.Debug.Log($"【宝物移除mod】执行：AddPool{dataTreasure.Name}");
            }
            int newCount = treasures.Count;
            if (newCount == 0)
            {
                Toggle();
                return;
            }
            else
            {
                currentIndex = Mathf.Clamp(currentIndex, 0, newCount - 1);
                UpdateFrameTarget();
            }
        }
    }
}