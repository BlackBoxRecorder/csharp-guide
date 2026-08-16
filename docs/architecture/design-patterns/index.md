---
title: 设计模式
description: C# 设计模式教程，按创建型、结构型、行为型三大类介绍 GoF 设计模式的 C# 代码实现。
---

# 设计模式

设计模式（Design Pattern）是解决特定场景下重复出现问题的成熟方案，是前人在大量工程实践中总结出的经验结晶。本栏目介绍经典的 GoF 设计模式，示例代码位于仓库 `src/DesignPatterns` 目录，每个模式一个独立工程，可单独编译运行。

## 创建型模式

创建型模式关注对象的创建机制，使创建逻辑与使用逻辑解耦。

| 模式 | 意图 | 代码位置 |
| :--- | :--- | :--- |
| Abstract Factory（抽象工厂） | 创建一组相关或依赖的对象，无需指定具体类 | `src/DesignPatterns/FactoryPattern/Abstract Factory` |
| Builder（建造者） | 分步构建复杂对象，将构造过程与表示分离 | `src/DesignPatterns/BuilderPattern` |
| Factory Method（工厂方法） | 将对象创建延迟到子类，由子类决定实例化哪个类 | `src/DesignPatterns/FactoryPattern/Factory Method` |
| Prototype（原型） | 通过复制现有实例创建新对象，避免重复初始化 | `src/DesignPatterns/PrototypePattern` |
| Singleton（单例） | 确保一个类只有一个实例，并提供全局访问点 | `src/DesignPatterns/SingletonPattern` |

## 结构型模式

结构型模式关注类与对象的组合方式，形成更大的结构。

| 模式 | 意图 | 代码位置 |
| :--- | :--- | :--- |
| Adapter（适配器） | 将不兼容的接口转换为客户端期望的接口 | `src/DesignPatterns/AdapterPattern` |
| Bridge（桥接） | 将抽象与实现分离，使两者可以独立变化 | `src/DesignPatterns/BridgePattern` |
| Composite（组合） | 将对象组织成树形结构，使单个对象与组合对象使用一致 | `src/DesignPatterns/CompositePattern` |
| Decorator（装饰器） | 动态地为对象添加职责，比继承更灵活 | `src/DesignPatterns/DecoratorPattern` |
| Facade（外观） | 为复杂子系统提供统一的高层接口 | `src/DesignPatterns/FacadePattern` |
| Flyweight（享元） | 共享细粒度对象，减少内存占用 | `src/DesignPatterns/FlyweightPattern` |
| Proxy（代理） | 为对象提供替身，控制对其的访问 | `src/DesignPatterns/ProxyPattern` |

## 行为型模式

行为型模式关注对象之间的职责分配与通信。

| 模式 | 意图 | 代码位置 |
| :--- | :--- | :--- |
| Chain of Responsibility（责任链） | 将请求沿处理链传递，直到有对象处理它 | `src/DesignPatterns/ChainOfResponsibilityPattern` |
| Command（命令） | 将请求封装为对象，支持撤销、重放与排队 | `src/DesignPatterns/CommandPattern` |
| Iterator（迭代器） | 顺序访问集合元素，而不暴露内部表示 | `src/DesignPatterns/IteratorPattern` |
| Mediator（中介者） | 通过中介对象简化对象间的交互 | `src/DesignPatterns/MediatorPattern` |
| Observer（观察者） | 定义一对多依赖，状态变化时自动通知订阅者 | `src/DesignPatterns/ObserverPattern` |
| State（状态） | 对象内部状态变化时改变其行为 | `src/DesignPatterns/StatePattern` |
| Strategy（策略） | 定义一系列算法并封装，使它们可以互相替换 | `src/DesignPatterns/StrategyPattern` |
| Template Method（模板方法） | 定义算法骨架，将步骤延迟到子类实现 | `src/DesignPatterns/TemplatePattern` |
| Visitor（访问者） | 在不修改类的前提下为类层次增加新操作 | `src/DesignPatterns/VisitorPattern` |

各模式可独立阅读，建议从创建型模式入手，理解"针对接口编程"的思想，再学习结构型与行为型模式。
