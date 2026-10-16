# 背包系统 Demo 恢复与架构说明

## 项目入口

作品集版本的入口场景为：

`Assets/InventoryDemo/InventoryDemo.unity`

旧场景 `Assets/Scenes/Inventory System Scene.unity` 仅作为历史交互参考保留，
不参与当前 Demo 的 MVC 组合与运行流程。

## 已完成的清理

- 删除 `Assets/ArtRes`，物品显示改为简单、易读的 Demo 图标和 UGUI 基础组件。
- 删除 `Assets/Plugins` 以及项目原有的 SQLite/MySQL 数据库依赖。
- 删除旧的数据库、数据管理、UI、Inventory 和 Player 脚本目录。
- 删除重复或不再使用的旧 Demo 类，避免多套背包逻辑并存。
- 确认 `Assets` 目录下没有空文件夹。
- 编辑器代码只保留场景构建器以及必要的 ScriptableObject 配置辅助逻辑。

SQLite 原生 DLL 在清理时被正在运行的 Unity 编辑器占用，因此临时移动为项目根目录下的
`sqlite3.dll.locked`。它已经不在 `Assets` 中，不会被 Unity 导入，也不会参与 Demo 运行。
完全关闭 Unity、确认 DLL 句柄释放后，即可删除该文件。

## 总体架构

本 Demo 使用：

- MVC：隔离运行时数据、业务规则与界面显示。
- UGUI：实现面板、按钮、文本、槽位和拖拽交互。
- ScriptableObject：配置物品、锻造配方和 Demo Catalog。
- 组合根：集中创建并连接运行时对象，避免各模块自行查找和创建依赖。

数据流如下：

`UGUI View -> Controller -> Model -> Changed 事件 -> UGUI View 刷新`

View 不直接修改 Model；跨区域拖拽、刷新、丢弃、拆分和锻造均通过 Controller 执行。

## Model 层

### InventoryModel

表示一个固定容量的运行时物品区域，不依赖场景、UGUI 或持久化系统。

职责包括：

- 添加物品与自动堆叠。
- 判断剩余容量。
- 指定槽位放置或堆叠。
- 删除指定数量。
- 槽位移动、交换与合并。
- 拆分堆叠。
- 清空区域。
- 数据变化时触发 `Changed` 事件。

### InventorySession

统一保存一次 Demo 运行期间的四个 `InventoryModel`：

- `Refresh`：刷新物品区域。
- `Backpack`：玩家背包。
- `ForgeInput`：锻造材料输入区。
- `ForgeOutput`：锻造成品输出区。

`InventoryArea` 枚举明确标识每个区域，避免使用含义不清晰的字符串或数字。

### ItemSlot

表示单个槽位中的运行时状态，保存：

- 当前 `ItemData` 引用。
- 当前堆叠数量。

它只负责单槽位状态，不负责跨槽位或跨区域业务。

## Controller 层

### InventoryController

背包操作的统一应用服务，负责跨区域转移、同区域移动、交换、堆叠、丢弃、拆分、
查询槽位以及向界面发布操作反馈。

### RefreshController

根据 `InventoryCatalog` 中配置的 ItemData 和刷新权重生成物品。

刷新区域完全按照 Catalog 中每个 ItemData 的 `spawnWeight` 进行加权随机选择，不再强制
填入某一条配方的固定材料。这样修改 ItemData、刷新权重或配方资源后，运行时刷新结果会
直接反映 ScriptableObject 的最新配置。

### CraftingController

负责锻造业务：

1. 使用 Forge Input 中的物品匹配 `ForgingRecipe`。
2. 检查 Forge Output 是否具有足够空间。
3. 消耗配方规定的材料数量。
4. 将锻造成品写入 Forge Output。
5. 发布成功或失败反馈。

### InventoryInteractionController

保存一次拖拽的来源区域和来源槽位，将 View 的放置请求转换为
`InventoryController.Transfer` 调用。槽位 View 因此不需要理解库存转移规则。

## View 层与组合根

### InventoryDemoBootstrap

整个 Demo 的组合根，在 `Awake` 中集中创建：

- `InventorySession`
- `InventoryController`
- `RefreshController`
- `CraftingController`
- `InventoryInteractionController`

场景对象只依赖 Bootstrap 暴露的接口，不自行创建业务对象，从而保持依赖关系清晰。

### InventoryAreaView

可复用的 UGUI 区域 View，负责根据 Model 容量实例化槽位、订阅 `Changed` 事件、
重新渲染对应区域、接收区域级放置事件，并将拆分和拖拽意图转交给 Controller。

### InventorySlotView

单个 UGUI 槽位，只负责显示和输入转发：

- 显示物品图标或简易颜色标识。
- 显示堆叠数量。
- 左键拖拽物品。
- 右键丢弃物品。
- 中键拆分一半堆叠。
- 接收槽位级放置事件。

### InventoryDemoUICommands

场景与运行时 Controller 之间的命令适配器。UGUI Button 使用持久化 UnityEvent 调用：

- `RefreshItems()`
- `Craft()`

这样重新加载场景后按钮绑定仍然有效，也避免在场景文件中直接引用纯 C# Controller。

### InventoryDemoFeedbackView

订阅 `InventoryController.Feedback`，在界面底部显示刷新、移动、拆分、丢弃、锻造成功
或失败等操作结果。

### InventoryDemoSceneBuilder

编辑器专用场景构建器，负责生成并连接 Canvas、EventSystem、Camera、Directional Light、
四个库存区域、槽位 Prefab、按钮、反馈文本、持久化 UnityEvent 和 Catalog 引用。

编辑器代码位于 `Assets/Editor`，不会进入运行时程序集。

## ScriptableObject 配置

### ItemData

每一种物品对应一个 ItemData 资源，配置唯一物品 ID、显示名称、描述、图标、
最大堆叠数量和刷新权重。

### ForgingRecipe

每条锻造配方配置配方名称、一个或多个 `ItemRequirement`、材料数量、输出物品和输出数量。

### InventoryCatalog

当前 Demo 的统一配置入口，持有全部 ItemData 与 ForgingRecipe 引用。

场景构建器会扫描：

- `Assets/SO/Items`
- `Assets/SO/ForgingRecipes`

并重建 `Assets/InventoryDemo/Resources/Inventory Catalog.asset`，确保引用正确序列化。

当前绑定审计结果：

`items=12, recipes=5, nullItems=0, nullRecipes=0, nullRequirements=0`

## 完整操作流程

1. 打开 `Assets/InventoryDemo/InventoryDemo.unity` 并进入 Play Mode。
2. Refresh Items 会自动随机生成物品，也可以点击 `Refresh Items` 重新刷新；每次刷新
   都会重新进行加权随机选择。
3. 使用左键将物品从 Refresh Items 拖入 Backpack。
4. 将符合配方的材料从 Backpack 拖入 Forge Input。
5. 点击 `Craft`。
6. 配方材料被消耗，成品出现在 Forge Output。
7. 将成品从 Forge Output 拖回 Backpack。
8. 右键点击物品可以丢弃；中键点击数量大于 1 的堆叠可以拆分。

右键丢弃在槽位的 Pointer Down 阶段执行，只有左键允许开始拖拽，避免右键输入被拖拽
事件取消。丢弃前会先保存物品名称，再清空槽位，确保反馈消息不会访问已经清空的 ItemData。

拖到指定槽位时会尝试放置、合并或交换；拖到区域空白位置时会自动放入第一个可用位置。

## Unity MCP 验证结果

已通过 Unity MCP 在真实 Play Mode 下执行完整回归测试：

`refreshOk=True, refreshSlots=6, materials=True, craftButton=True, output=1, return=True`

结果含义：

- 成功刷新 6 个物品槽位。
- 成功将刷新物品转移到背包。
- 成功将配方材料转移到锻造输入区。
- 成功调用场景中的真实 Craft Button UnityEvent。
- 成功生成 1 个锻造成品。
- 成功将锻造成品拖回背包。

其他验证结果：

- 堆叠拆分：`split=True`。
- 场景缺失脚本：`0`。
- 损坏 Prefab：`0`。
- 最终编译完成，Unity 控制台无新增错误。
- `Assets/ArtRes` 与 `Assets/Plugins` 均不存在。
- `Assets` 下无空文件夹。
- 连续 5 次刷新结果均发生变化，证明刷新不再固定使用第一条配方材料。
- 右键删除验证：真实 Backpack 槽位从 `before=1` 变为 `empty=True`。

最终验证截图：

`Assets/Screenshots/InventoryDemo_FinalVerified.png`

## 迁移到其他项目

迁移该背包系统时，复制以下内容：

- `Assets/Scripts/InventoryDemo`
- `Assets/Scripts/Item`
- `Assets/InventoryDemo` 中需要的 Prefab、Resources 和简单图标
- `Assets/SO` 中的物品及配方配置

如果目标项目已有自己的场景 UI，可以不复制 Demo 场景，而是在目标场景中放置 Bootstrap、
AreaView、按钮命令适配器和反馈 View，并重新绑定引用。

运行时代码不依赖数据库、旧单例管理器或原美术资源。Model、Controller、View 与配置资源
职责独立，可以分别替换或扩展。

## 扩展建议

- 增加装备区域时：新增 `InventoryArea` 和对应 Model/View，不修改槽位显示核心逻辑。
- 增加保存功能时：单独实现 Repository 或 Save Service，在边界层转换库存数据，不让
  `InventoryModel` 直接依赖数据库。
- 增加物品提示框时：由新的 View 读取 ItemData，不把提示框逻辑写入 Model。
- 增加更多配方时：创建新的 ForgingRecipe 资源，不需要修改 CraftingController。
- 替换正式美术时：只替换 ItemData 图标和 UGUI 样式，不修改背包业务规则。
