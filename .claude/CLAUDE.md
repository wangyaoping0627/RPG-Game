# 2D Action Game Project

> 项目规则 + 全局规则引用

## 全局规则
@C:\Users\29342\.claude\CLAUDE.md

## 本项目信息
- 项目路径：D:\UnityProject\2DGame
- 类型：Unity 2D 动作游戏（Tiny Swords 资源包）
- 脚本目录：Assets/Scrpits/
- 场景：Assets/Scenes/ (SampleScene, Start)
- 美术资源：Assets/Sources/ (Tiny Swords)、Assets/Spirits/
- CodeGraph：已索引 ✅
- 目标：做完做成简历项目

## 本项目约定
- 所有代码自己写的能看懂
- 改代码前先说思路
- 一次只做一个功能点
- 做项目同时顺便深挖数据结构（用项目当教材，不额外抽时间啃书）

---

## 项目现状（2026-06-08）

### 已有功能
| 系统 | 状态 |
|---|---|
| 玩家移动（WASD + 加速 + 击退）| ✅ |
| 玩家攻击（冷却 + 范围判定）| ✅ |
| 敌人 AI（追击 + 攻击 + 击退）| ✅ 基本能用 |
| 血量系统（玩家 + 敌人）| ✅ |
| 数值管理（StatsManager 单例）| ✅ |
| 菜单暂停 / 数值查看 | ✅ |
| 场景切换 | ✅ |
| 地形遮挡（进出山渲染层切换）| ✅ |
| 经验升级系统 | ⚠️ 框架写好，按回车加经验，没接到杀敌上 |

### 已知问题
1. `EnemyCombat.cs` — 空类，敌人攻击逻辑塞在 `EnemyMovement` 里，职责混了
2. `PlayerCombat.DealDamage()` — 只打 `enemies[0]`，多个敌人在范围内只打第一个
3. 经验系统没连接到杀敌 — 现在按回车键手动加经验
4. 玩家死了 `SetActive(false)` 直接消失 — 没重生、没游戏结束
5. 敌人死了没重生，没有胜利条件

---

## 待办功能

| # | 内容 | 涉及文件 |
|---|---|---|
| 1 | 杀敌给经验，升级加属性 | ExpController + EnemyHealth |
| 2 | 玩家死了弹「重来」而不是直接消失 | PlayerHealth |
| 3 | 清完敌人算赢 / 切下一关 | 新脚本或 EnemyHealth 里计数 |
| 4 | EnemyCombat 空类处理 — 把 Damage() 搬进去或删掉空文件 | EnemyMovement + EnemyCombat |
| 5 | 攻击打所有范围内的敌人，不只是 `enemies[0]` | PlayerCombat |

---

## Gameplay 开发自学清单

> 每项在做项目时自然碰到，顺便深挖一个概念。做完一项把 ▢ 改成 ✅。

▢ **1. 数组和遍历**
- 触发：改 `enemies[0]` 为遍历所有敌人时
- 搞懂：数组连续内存、索引访问 O(1)、foreach vs for
- 落地：攻击范围内所有敌人都受伤

▢ **2. enum 替代多个 bool**
- 触发：EnemyMovement 里 `isChasing` / `isAttack` / `isStand` 三 bool 互斥
- 搞懂：为什么互斥状态用 enum 而不是 bool、状态机雏形
- 落地：把敌人状态换成 `EnemyState` enum

▢ **3. 委托/事件**
- 触发：EnemyHealth 里敌人死了，ExpController 怎么知道？
- 搞懂：`delegate`、`event`、`Action`、`+=` 订阅、`?.Invoke` 广播
- 落地：敌人死亡发事件，ExpController 接收加经验

▢ **4. Dictionary**
- 触发：StatsUI 里 UpdateDamage / UpdateSpeed / UpdateMaxHealth 三个方法几乎一样
- 搞懂：键值对、哈希原理、O(1) 查找、什么时候用 List 什么时候用 Dictionary
- 落地：用一个 Dictionary 存所有属性，UI 循环显示

▢ **5. List 和泛型**
- 触发：一波敌人不止一个，需要管理敌人列表
- 搞懂：List<T> 动态扩容、Add/Remove/Contains、和数组的区别
- 落地：场景里统一管理所有敌人，全死了判断胜利

▢ **6. ScriptableObject（数据驱动）**
- 触发：每种敌人血量、速度、伤害不一样，别写死在脚本里
- 搞懂：数据和逻辑分离、ScriptableObject 的创建和引用
- 落地：给敌人做一份属性配置表（SO），不同敌人引用不同配置

▢ **7. 协程（Coroutine）**
- 触发：已经在用 `StartCoroutine(UnlockedMove(stunTime))`
- 搞懂：IEnumerator、yield return、和 Update 的区别
- 落地：攻击冷却、击退硬直 — 已经用了，回头搞懂原理

▢ **8. 继承和组合**
- 触发：如果有多种敌人（近战、远程）— 一样的血量逻辑、不一样的攻击方式
- 搞懂：基类抽共性、子类加差异、组合优于继承
- 落地：抽象一个 Enemy 基类，近战和远程各继承

---

## 背景决策

- **学历问题：** 大厂校招简历关大概率过不了，但中小工作室/外包公司/创业团队更看项目。策略：拼项目攒资本，两年后经验权重 > 学历权重。
- **方向：** Gameplay 开发（玩家操控、AI、战斗系统）。TA 和美术已排除。
- **基础课优先级（Gameplay 方向）：** 数据结构 ★★★★★ → 计组 ★★★ → 操作系统 ★★ → 网络看项目。
- **本项目选择：** 放弃了 Kinect 项目和重开新项目的想法，决定把这个 2D 动作项目做完。
