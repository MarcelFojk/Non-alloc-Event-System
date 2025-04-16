# ⚡ Non-Alloc Event System

A blazing-fast, zero-allocation, low-level event system for C# / Unity with full support for event consumption and `ref`-based struct data.

---

## ✨ Why this exists

In performance-critical environments like **Unity**, memory allocations can lead to **GC spikes** and frame hitches. The default `Action<T>`-based or `event`-based systems in .NET / Mono create allocations when listeners are added or removed — and worse, this does **not scale linearly**.

### ❌ The problem with `Action -= handler`

- Each removal (`-=`) of a listener allocates about **104 bytes**.
- But **1000 removals** don’t allocate ~100 KB — they can allocate **up to ~4 MB**.
- Why? `MulticastDelegate` stores its invocation list as a **binary tree**. Rebuilding it becomes more expensive as it grows.

📌 In Unity, `-=` often happens during **scene transitions**, when objects unregister themselves from global events — which leads to **unpredictable alokacje** and **longer loading times**.

This system avoids that entirely by using a fixed-size listener array and raw `delegate` storage, bypassing `+=`/`-=` altogether.

---

## 🧠 Key Features

- 🧊 **Zero allocations** for registration, unregistration, and invocation
- 🧠 Supports passing event data **by `ref`**, avoiding struct copies
- 🔥 Event consumption (`Consumed = true`) stops further listener calls
- 🛑 Avoids `MulticastDelegate`, `event`, and `Action<T>` overhead
- 🧰 Optional **reflection-based** invocation for tools (not runtime)
- 📉 No closures, no lambdas, no dynamic delegate generation

---

## 📊 Allocation Benchmark

| Scenario                         | Operations | Total Allocation |
|----------------------------------|------------|------------------|
| `Action -= handler` (System)     | 1          | ~104 B           |
| `Action -= handler` (System)     | 1000       | ~4 MB (!)        |
| `NonAllocEvent.Register()`       | 1000       | **0 B ✅**        |
| `NonAllocEvent.Invoke()`         | 1000       | **0 B ✅**        |

---

## 🚀 Usage

### Step 1 – Define event data

Event data must be a `struct` implementing `IConsumableData`:

```csharp
public struct MyEventData : IConsumableData
{
    public int Value;
    public bool Consumed { get; set; }
}
```

---

### Step 2 – Create a concrete event class

```csharp
public class MyEvent : NonAllocEvent<MyEventData>
{
    public MyEvent(int capacity) : base(capacity) { }
}
```

---

### Step 3 – Register listeners

⚠️ Listeners must match the custom delegate signature: `void(ref T data)`.

```csharp
void OnEvent(ref MyEventData data)
{
    Console.WriteLine($"Value: {data.Value}");
    data.Consumed = true; // stop propagation
}

var evt = new MyEvent(10);
evt.Register(OnEvent);
```

---

### Step 4 – Invoke the event

```csharp
evt.Invoke(new MyEventData { Value = 42 });
```

---

### 🛑 Important Constraints

- No support for lambda expressions like `(ref T data) => { ... }` — due to C# delegate limitations (no `ref` lambdas).
- `T` must be a `struct` implementing `IConsumableData`.
- Reflection-based invocation is included for tooling/editor scripts only — **avoid it at runtime** for performance reasons.

---

## ✅ Benefits

- Fully GC-free during runtime usage
- Safe and deterministic event flow
- Ideal for real-time games, ECS-style architectures, and performance-heavy systems
- Reduces GC pressure during **scene transitions** in Unity

---

## 🧰 Implementation Overview

```csharp
public abstract class NonAllocEvent<T> where T : struct, IConsumableData
{
    private readonly RefAction<T>[] listeners;
    private int listenerCount = 0;

    public void Register(RefAction<T> listener) { ... }
    public void Unregister(RefAction<T> listener) { ... }

    public void Invoke(T data)
    {
        for (int i = 0; i < listenerCount; i++)
        {
            listeners[i]?.Invoke(ref data);
            if (data.Consumed) break;
        }
    }
}
```

---

## 🧪 Example Event

```csharp
public struct DamageData : IConsumableData
{
    public int Amount;
    public bool Consumed { get; set; }
}

public class DamageEvent : NonAllocEvent<DamageData>
{
    public DamageEvent(int capacity) : base(capacity) { }
}
```

```csharp
void BlockHighDamage(ref DamageData data)
{
    if (data.Amount > 100)
        data.Consumed = true;
}

var damageEvent = new DamageEvent(5);
damageEvent.Register(BlockHighDamage);

damageEvent.Invoke(new DamageData { Amount = 150 });
```

---

## 📦 Installation

Clone the repo or copy the core files into your Unity or C# project:

```bash
git clone https://github.com/MarcelFojk/Non-alloc-Event-System
```

---

## 📄 License

MIT – do what you want, just don’t forget where it came from 🚀