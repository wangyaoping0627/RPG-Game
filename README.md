# RPG — 项目架构说明

2D 俯视角动作 RPG（Unity 2022.3.62f3c1）。核心循环：打怪 → 爆装备 → 拾取 → 穿戴变强 → 挑战更强的怪。

## 目录结构

```
Assets/
├── Scenes/
│   ├── Start.unity          开始场景（LoadScene 跳转）
│   └── MainScene.unity      主游戏场景
├── Scripts/                 按模块分目录（无 asmdef，全部编译进默认程序集）
│   ├── Core/                全局基础设施
│   │   ├── StatsManager     玩家属性单例（跨场景常驻，装备/升级都写回这里）
│   │   └── Main             入口：设置目标帧率
│   ├── Player/              玩家：移动 / 攻击（动画事件驱动判定）/ 生命
│   ├── Enemy/               敌人：状态机(待机→追击→攻击→受击→死亡) / 攻击 / 生命
│   ├── Items/               道具与掉落链路（一条完整数据链）
│   │   ├── ItemData         道具 SO 资产定义（右键 Create > RPG/Item）
│   │   ├── ItemStack        背包格子（数据+数量，可堆叠）
│   │   ├── Inventory        背包单例（静态类，40 格上限）
│   │   ├── EquipmentManager 装备单例（静态类，5 槽位，增量法写回 StatsManager）
│   │   ├── EnemyLoot        敌人掉落表组件（挂在敌人身上）
│   │   ├── PickupSpawner    死亡时生成掉落物（运行时动态创建，无需 prefab）
│   │   └── PickupItem       掉落物自动拾取组件
│   ├── Combat/              战斗表现与计算（无状态工具 + 效果组件）
│   │   ├── CombatCalculator 伤害计算（攻防/暴击/浮动）
│   │   └── 飘字/顿帧/震屏   打击感三件套
│   ├── UI/                  现有 UI（经验条、属性面板）
│   └── World/               场景工具（场景加载、山体碰撞区）
└── 插件/AVProVideo          视频插件（备用）
```

## 核心数据流

```
EnemyHealth.EnterDeath()
  → EnemyLoot.SpawnDrops(尸体位置)          ← 按掉落表随机
  → PickupSpawner.Spawn()                   ← 动态生成掉落物
  → PickupItem.OnTriggerEnter2D(Player)     ← 自动拾取
  → Inventory.Add()                         ← 入包（40格上限）
  → EquipmentManager.Equip()                ← 穿戴/脱下
  → 增量写回 StatsManager 属性              ← 攻击力/防御/移速/血量/暴击/范围
  → PlayerCombat / PlayerMove 读取生效
```

## 事件约定（UI 从这里拿数据）

| 事件 | 声明处 | 触发时机 |
|---|---|---|
| `EnemyHealth.OnAnyEnemyDeath(int exp)` | EnemyHealth | 任意敌人死亡（经验用） |
| `EnemyHealth.OnDeath(int exp)` | EnemyHealth | 单个敌人死亡（实例事件） |
| `StatsManager.OnStatsChanged` | StatsManager | 升级/装备/属性变化 |
| `Inventory.OnChanged` | Inventory | 背包增删 |
| `EquipmentManager.OnChanged` | EquipmentManager | 装备槽变化 |

UI 订阅后必须在 `OnDestroy` 退订（参照 `UI/ExpController.cs` 的写法）。

## 关键约定

- **属性唯一出口**：玩家所有数值存在 `StatsManager`，装备/升级都通过它写回，战斗代码只读。
- **单例两种形态**：场景组件用 MonoBehaviour 单例（`StatsManager` 等）；纯数据管理器用静态类（`Inventory`、`EquipmentManager`，无场景对象，后续存档时自行序列化其内部状态）。
- **掉落物无需 prefab**：`PickupSpawner` 运行时用 `GameObject`+组件拼装，美术图标就位后改 `PickupSpawner` 一处即可。
- **品质倍率**：`ItemData` 的 `XxxValue` 属性自动套用 `QualityConfig` 倍率，改数值只改 SO 资产。

## 状态

- ✅ 已有：移动/攻击/击退硬直/敌人状态机/经验升级/伤害飘字/顿帧/震屏/掉落/拾取/背包/装备
- 🚧 未做：消耗品增益、多种敌人、刷怪点、Boss、完整 UI（背包/装备面板）、存档、音效
