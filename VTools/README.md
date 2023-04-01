# 微风的工具库
本模组旨在为游戏《觅长生》的其他模组提供前置工具

## VTools
对于C#模组，可以直接调用VTools的方法。<br>
（相信你知道怎么看）

## VNext
扩展补充Next模组的[**对话指令**](#vnextdialogevent "VNext.DialogEvent")、[**触发器**](#vnextdialogtrigger "VNext.DialogTrigger")、[**环境脚本**](#vnextdialogenvquery "VNext.DialogEnvQuery")，来帮助模组作者们更方便地编写剧情类模组。

## VNext.DialogEvent
**对话指令**

|指令|说明|特点|范例|
|---|---|---|---|
| **SendNewEmail**<br>`SendNewEmail*联系人id#邮件内容#发送日期#邮件类型id#物品id#物品数量#过期月数` |  1.联系人id为数字，必填 <br>2.邮件内容 为字符串，必填 <br>3.发送日期可留空可省略，则默认为当前游戏内时间。 <br>4.邮件类型id可省略，默认为0表示不带物品，1表示NPC向玩家发送物品，2表示NPC向玩家请求物品 <br>5.物品id可省略，默认为0，当邮件类型id为1或2时有效且必填 <br>6.物品数量可省略，默认为0，当邮件类型id为1或2时有效且必填 <br>7.过期月数可省略，默认为60，是指当邮件类型id为2时，月数内NPC回复“正是我急需”，超过月数则“已取得替代物”  | 1.联系人和发信人必定一致，可选择是否携带物品或请求物品 <br>2.无论发多少封不同内容的邮件，都只占用对白表id100000号  <br>3.邮件类型2当玩家提交物品时，NPC固定+1好感，按物品总价值加情分 | `"SendNewEmail*609#啊这。。（脸红）"` 倪旭欣发送一句话的邮件 <br>`"SendNewEmail*609#那个，我有东西要送给你##1#5211#1"` 倪旭欣发送带一个丹药物品的邮件 <br>`"SendNewEmail*615#兄弟，我撸铁时肌肉拉伤了，大夫说一年之内必须服用金刚铁骨丹，求帮忙啊！##2#5306#2#12"` 百里奇求丹药 |
| **SendOldEmail**<br>`SendOldEmail*联系人id#发信人id#邮件内容#发送日期`| 1.联系人id为数字，必填 <br>2.发信人id为数字，必填 <br>3.邮件内容 为字符串，必填 <br>4.发送日期可留空可省略，则默认为当前游戏内时间。仅为显示，实际是立即发送的，格式为DateTime转化的字符串格式 | 1.联系人和发信人可不相同，形成类似“群聊”的效果，不可携带物品，不可带任务。 <br>2.每有一个发信人需占用一个传音符id100000 + SenderNPCid | `"SendOldEmail*609#2#你小子看上我们姑娘了吧？"` 魏老在联系人倪旭欣下发信 <br>`"SendOldEmail*609#621#小子你又皮痒了是吧？"` 倪振东在联系人倪旭欣下发信  <br>`"SendOldEmail*614#1646#宝贝女儿\r\n当你出生的时候我就写下了这封信#0001/01/01"` 林沐心展示她父亲在1年1月1日写的信 |
| **SendNTaskEmail**<br>`SendNTaskEmail*联系人id#委托任务id#邮件内容#发送日期#是否强制刷新` | 1.联系人id为数字，必填 <br>2.委托任务id为数字，必填 <br>3.邮件内容 为字符串，必填 <br>4.发送日期可留空可省略，则默认为当前游戏内时间。 <br>5.是否强制刷新为布尔类型，可省略，默认为false <br>6.游戏内的任务分为**传闻任务**（主线支线）和**委托任务**（可反复接取完成） <br>7.本邮件可实现发放**委托任务**邮件。需要作者在《task》配置文件中事先查阅好nTaskId，委托任务包括主城委托、宗门委托、随机副本、天机阁情报等等。 <br>8.“任务大类”表中id即为nTaskId，发邮件前会随机生成子任务，即按照“详细任务随机范围”去“详细任务”表中按类型进行匹配，不强制刷新的意思是上次随机生成后还在cd中就不再次生成 | 1.通过邮件向玩家发放委托任务，玩家点击邮件就接受。2.每有一个委托任务id需占用一个传音符id200000 + nTaskId  <br>3.**【注意】** 子任务有“境界区间”要求，有可能恰好玩家的境界没有任何一个子任务是符合的，就会生成子任务失败，不会发送邮件|`"If*[&GetLevel()>1&]#SendNTaskEmail*609#150#对了！我想邀请你一起去除妖！"` 当玩家境界高于练气初期时，给玩家发放“除妖”委托任务。经查表，此任务的子任务“境界区间”条件没有1级的 <br>`"If*[&player.menPai==4&]#SendNTaskEmail*301#703#授业长老生病了，你能代替他去完成授业任务吗？"` 当玩家门派为星河剑派时，给玩家发放星河的授业长老任务。经查表，各门派长老任务id不同 |
| **AddShengWang**<br>`AddShengWang*势力id#声望增加值`| 1.势力id为数字，必填 <br>2.声望增加值为数字，可正可负 | 1.势力id可在配置表《str》"@势力好感度名称表"表中查询 | `"AddShengWang*0#-100"` 减少宁州声望100 <br>`"AddShengWang*19#100"` 增加无尽之海声望100  |
| **CreateOneNpc**<br>`CreateOneNpc*类型#流派#境界#性别#正邪`| 1.所有参数为可选值，若不指定就留空或者写0 <br>2.类型、流派、境界可在配置表《AvatarAI》@NPC类型表中查询 <br>3.**若表中模板筛选出来没有一条全条件都符合的，则会创建失败** <br>4.性别0表示不指定性别，1男2女，类型3星河会强制设为女 <br>5.正邪0表示不指定，1正2邪  | 1.可任意组合你要指定的条件 <br>2.创建成功后，环境属性roleID、roleName即为创建出来的npc <br>3.若创建失败，环境属性roleID会变成0 | `"CreateOneNpc*#33#6#2#1"` 创造了一个离火化焰流派筑基后期正性格的女修士 <br>`"CreateOneNpc*14#0#8#0#2"` 创造一个禾山道金丹中期邪性格修士  |
| **SearchOneNpc**<br>`SearchOneNpc*类型#流派#境界#性别#正邪`| 1.所有参数为可选值，若不指定就留空或者写0 <br>2.类型、流派、境界可在配置表《AvatarAI》@NPC类型表中查询 <br>3.**若已有NPC中筛选出来没有一条全条件都符合的，则会筛选失败** <br>4.性别0表示不指定性别，1男2女 <br>5.正邪0表示不指定，1正2邪 <br>6.筛选已有npc仅包括实例NPC，**不包括已飞升和已失联的npc** | 1.可任意组合你要指定的条件 <br>2.筛选成功后，**本对话指令只会返回其中随机一个结果**，环境属性roleID、roleName即为筛选出来的npc <br>3.若筛选失败，环境属性roleID会变成0 | `"CreateOneNpc*#33#6#2#1"` 随机选择一个离火化焰流派筑基后期正性格的女修士 <br>`"SearchOneNpc*11##1##1"` 随机选择一个白帝楼类型炼气初期正性格修士  |
| **NpcDoAction**<br>`NpcDoAction*npcId#actionId`| 1.NPC强制执行actionId <br>2.npc所在地点会变成执行actionId可能去往的地点之一（与npc类型有关） <br>3.npc信息里的行动会变成所执行的actionId <br>4.执行actionId大多会获取收益（钱、经验、物品等），还有部分actionId是下一月获得收益，也就是说执行一次相当于此npc会比正常情况多获得一次收益  | 1.**本对话指令仅对实例NPC生效，包括有绑定的固定npc，不包括没绑定的工具人NPC** <br>2.**本对话指令仅包括部分可执行actionId，并非全部行动都可执行** <br>主要是配置表《AvatarAI》@NPC行动表中131号以前的 <br>3.执行结果可使用临时参数`GetArg("NpcDoAction")`查看，1为成功0为失败 | `"NpcDoAction*[&roleID&]#51"` 让roleID去东石谷坊市跑商 <br>`"NpcDoAction*609#5"` 让倪旭欣去炼丹（**慎重修改固定NPC的行动id，可能会影响主线剧情，而且部分特殊行动id本指令无法改回**）  |
| **NpcMapRemoveNpc**<br>`NpcMapRemoveNpc*npcId`| 1.从三类NPC地图中移除NPC，包括大地图、三级场景、副本，使之无法用正常方法被找到交互    | 1.**本对话指令仅对实例NPC生效，包括有绑定的固定npc，不包括没绑定的工具人NPC**  <br>2.执行结果可使用临时参数`GetArg("NpcMapRemoveNpc")`查看，1为成功0为失败 | `"SetInt*npcId#[&roleID&]","NpcMapRemoveNpc*[&GetInt(\"npcId\")&]"`  从地图上移除npcId对应的NPC |
| **CreateDongFu**<br>`CreateDongFu*dongFuID*level`| 1.按dongFuID创建设定level灵眼等级的洞府 <br>2.若已有此id的洞府，则等效于修改洞府灵眼等级   | 1.**推荐先用环境脚本`PlayerHasDongFu`确认玩家是否有此id的洞府**   <br>2.逸风城购买洞府id为1，门派金丹赠送后山洞府为2  <br>3.洞府id类型为int， 理论上可以有-2,147,483,648 到 2,147,483,647号洞府 | `"CreateDongFu*5#3"`  创建灵眼等级为3的五号洞府 |
| **SetDongFuName**<br>`SetDongFuName*dongFuID*name`| 1.按dongFuID修改洞府的名字    | 1.**请先用环境脚本`PlayerHasDongFu`确认玩家是否有此id的洞府**    | `"SetDongFuName*5#五号三级"`  给五号洞府改名 |
| **SetNowDongFuID**<br>`SetNowDongFuID*dongFuID`| 1.在玩家传送进洞府场景"S101"前，通过此指令设定传进几号洞府    | 1.**一般和`PlayerWarp`指令搭配使用**    | `"SetNowDongFuID*5"`  接下来传送进的是五号洞府 |
| **NpcWarp**<br>`NpcWarp*npcId#场景#index`| 1.将NPC传送至指定场景，原有位置会移除 <br>2.场景为"AllMaps"时，表示传送到大地图，index表示在大地图上路点序号 <br>3.场景为"F"开头时，表示传送到固定副本中，index副本中格子序号；若玩家已在此副本中且index留空或为0，表示传送到玩家当前格子。 <br>4.场景为"S"开头时，表示传送到三级场景中，若场景为"S101"表示洞府，则index表示NPC传到几号洞府，其他情况index无含义  | 1.**本对话指令仅对实例NPC生效，包括有绑定的固定npc，不包括没绑定的工具人NPC**    | `"NpcWarp*[&GetInt(\"npcId\")&]#AllMaps#21"`  将选定的npc传送到大地图21号位置(天机阁位置) <br>`"NpcWarp*[&GetInt(\"npcId\")&]#F26#0"` 将NPC传送到青石灵脉，玩家已在此副本时传动到玩家所在格子  <br>`"NpcWarp*[&GetInt(\"npcId\")&]#S22"` 将NPC传动到云汐城场景 |
| **PlayerWarp**<br>`PlayerWarp*场景#index`| 1.将玩家传送至指定场景 <br>2.场景为"AllMaps"时，表示传送到大地图，index表示在大地图上路点序号 <br>3.场景为"F"开头时，表示传送到固定副本中，index副本中格子序号；**此方法传送进副本出来后还在原地**。 <br>4.场景为"Sea"开头时，表示传送到海域上，index海域格子总序号，若留空或为0则为本指令默认位置 <br>4.场景为"S"开头时，表示传送到三级场景中。index大于零表示玩家从场景出来后到大地图上的指定位置，index留空或为0表示出来后在大地图的位置不变，-1表示由本指令自动设置出来后的位置（不完全正确，官方命名有部分规律但没完全遵守） <br>4.若场景为"S101"表示洞府，则提前用`SetNowDongFuID`指定要去的洞府  | 1.S开头的三级场景，没有强制绑定出来后到大地图的位置，也就是说进去前在路点几，出来还是原地  <br>2.官方并没有一个“三级场景出来应该在大地图什么位置”的对照表。从剧情的角度，推荐手动指定出来后在大地图的位置，或先用`"PlayerMove"`指令先在大地图上移动 <br>3. 海域位置详见说明，若传送海域不包含指定的index，则会出bug  | `"PlayerWarp*AllMaps#10"`  玩家传送到大地图10号位置 <br>`"PlayerWarp*F16#2"` 玩家传送到风雷谷2号位置（可以是正常无法走到的位置）  <br>`"PlayerWarp*Sea5#0"` 将玩家传送到千流海域默认位置 <br>`"PlayerWarp*S20#-1"` 将玩家传送到逸风城，且出来到大地图的位置由本指令修改|
| **PlayerMove**<br>`PlayerMove*index`| 1.玩家小人直线移动到指定的index <br>2.玩家在大地图、副本、海上都有效   | 1.**请作者确认玩家所在场景有指定的index** <br>2.尤其是海上，每个海域包含的index都不同，本海域位置可用此移动指令，若为跨海域建议使用传送  | `"PlayerMove*3217"`玩家在千流海域时移动到此指定位置  |
| **PlayerWalk**<br>`PlayerWalk*index`| 1.仅玩家在大地图有效，如同点击目的地 <br>2.玩家小人会自动寻路一段段移动到指定的index   | 1.**请作者确认玩家所在场景有指定的index** | `"PlayerWalk*20"` 玩家在大地图上时走到指定20位置 |
| **PlayerGetInRandomFuBen**<br>`PlayerGetInRandomFuBen*fuBenId`| 1.玩家传送进指定的随机副本 <br>2.随机副本可在配置表《EndlessSea》中@随机副本表查询   | 1.**env.mapScene和Tools.getScreenName()查出的是随机副本的uuid，与固定副本的F多少不同**  <br>2.要事后查询玩家在随机副本是哪个id（也就是本指令输入参数），可以使用环境脚本中玩家信息`player.NowRandomFuBenID` <br>3.要查询随机出的副本中文名，可用`VTools.GetPlaceName()` | `"PlayerGetInRandomFuBen*88"`玩家进入某个碎星海随机副本 |



## VNext.DialogTrigger
**触发器**<br>
**由于触发器触发较为频繁，建议需要时开启触发器，不需要时关闭触发器，并给触发器加上恰当的condition**

|指令|说明|特点|环境|
|---|---|---|---|
| **附近的人**<br>**OnNearNpc** | 1.**必须**把环境脚本`NearNpcContains`作为condition条件之一一起使用<br>2.当附近的人改变时触发，通过`NearNpcContains`判定有没有遇到设定的NPC，从而开启剧情事件 | 1.**队列触发器** <br>该类触发器触发的事件会逐项执行<br>2.屏蔽了反复进出空场景刷概率触发，可反复进出有人的场景刷概率触发<br>3.在洞府闭关，过月结算时有npc拜访也能触发 | 在使用环境脚本`NearNpcContains`作为第一个条件后，以下环境属性可使用<br>roleID<br>roleName<br>roleBindID<br>mapScene |
| **大地图移动前**<br>**BeforeAllMapMove** | 1.当玩家在宁州大地图移动前触发，**包括自动寻路的每段开头**，包括遁术飞行的开头，不包括到达目的地 <br>2.**一旦成功触发，会终止之后的移动** <br>3.若condition设置不当，可能会造成玩家频繁触发、寸步难行  | 1.**单项触发器** <br>该类触发器仅触发优先级最高且满足条件的事件  | 常和环境脚本`string GetCurMapRoad()`获取当前道路ID、`string GetRoadName(string roadId)`获取道路名称 搭配使用 |
| **副本移动前**<br>**BeforeFubenMove** | 1.当玩家在副本移动前触发，鼠标点击格子或wasd移动都可触发，不包括到达目的地 <br>2.**一旦成功触发，会终止之后的移动**  <br>3.若condition设置不当，可能会造成玩家频繁触发、寸步难行  | 1.**单项触发器** <br>该类触发器仅触发优先级最高且满足条件的事件  | 常和环境脚本`int GetCurFubenIndex()`取当前副本位置、`string GetCurScene()`获取当前场景id 搭配使用 |
| **结算完成**<br>**OnJieSuanComplete** | 1.当玩家结算完成后触发 <br>2.**此触发器推荐适合处理后台数据，如修改NPC和玩家的信息** <br>由于结算完成时间无法控制，因此若进行前台交互对话会显得突兀   | 1.**队列触发器** <br>该类触发器触发的事件会逐项执行<br>  | 常和环境脚本`DateTime GetNowTime()`获取DateTime格式的游戏当前时间、`bool Before/After(int year, int month, int day)`判断是否在某个日期之前之后 搭配使用 |

**范例**
``` 
    {
        "id":"遇见倪旭欣",
        "type":"附近的人",
        "condition":"NearNpcContains(609) && mapScene==\"S163\"",
        "triggerEvent":"偶遇",
        "default" : true,
        "once" : false
    }

    //在伴月楼遇到倪旭欣时开启剧情事件，使用NearNpcContains后环境属性mapScene即可使用
```
``` 
    {
        "id":"遇见百里奇或林沐心",
        "type":"附近的人",
        "condition":"NearNpcContains(Array(615, 614),50)",
        "triggerEvent":"偶遇",
        "default" : true,
        "once" : false
    }

    //当遇到百里奇或者林沐心时有50%概率开启剧情事件
```
```
    {
        "id":"偶遇",
        "character":{
            "主角" : 1,
            "倪旭欣" : 609,
            "林沐心" : 614,
            "百里奇" : 615
        },
        "dialog":[
            "Say*[&roleName&]#好巧，能在[&GetSceneName(GetCurScene())&]这儿碰到你。",
            "主角#确实。"
        ],
        "option":[

        ]
    }

    //前面已获得环境属性roleName可直接使用。
```
```
   {
        "id":"大地图移动前触发器1",
        "type":"大地图移动前",
        "condition":"GetTriggerCount(\"大地图移动前触发器1\") <=3 && RandomProbability(20)",
        "triggerEvent":"移动测试",
        "default" : true,
        "once" : false
    }
    
    //在大地图移动时有20%概率触发，且仅能触发3次。可修改触发器默认为关闭"default" : false，当剧情需要时再打开。
```
```
    {
        "id":"移动测试",
        "character":{
            "旁白" : 0,
            "主角" : 1
        },
        "dialog":[     
            "旁白#[&GetNPCName(1)&]正要离开[&GetRoadName(GetCurMapRoad())&]，突然发现地上有一袋灵石！",
            "ChangeMoney*1000",
            "AddShengWang*0#-100",
            "主角#我在宁州的声望是[&GetShengWang(0)&]！"
        ],
        "option":[

        ]
    }
```
```
    {   
        "id":"副本移动前触发器",
        "type":"副本移动前",
        "condition":"GetCurScene() == \"F26\" && RandomProbability(20)",
        "triggerEvent":"移动测试2",
        "default" : false,
        "once" : false
    }

    //此触发器默认为关闭，在开启后，在青石灵脉有20%概率触发。
    //提示，固定副本有场景id，随机副本场景id为随机生成的uuid。
```
```
    {
        "id":"移动测试2",
        "character":{
            "主角" : 1,
        },
        "dialog":[     
            "主角#我在[&GetPlaceName()&]副本第[&GetCurFubenIndex()&]位置发现了什么",
            "AddShengWang*19#100",
            "主角#我在无尽之海的声望是[&GetShengWang(19)&]！"
        ],
        "option":[

        ]
    }
```

```
    {
        "id":"结算触发器",
        "type":"结算完成",
        "condition":"true",
        "triggerEvent":"结算后",
        "default" : true,
        "once" : false
    }

    //这个范例中每次结算后都触发
```
```
    {
        "id":"结算后",
        "character":{
            "旁白" : 0,
            "主角" : 1
        },
        "dialog":[     
            "旁白#结算完成了",
            "CreateOneNpc*#33#6#2#2",
            "主角#创造了一个离火化焰流派筑基后期邪性格的女修士[&roleID&][&roleName&]",
            "NpcDoAction*[&roleID&]#51",
            "主角#并让他在东石谷坊市跑商",
            "SearchOneNpc*11##1##1",
            "If*[&roleID==0&]#主角#没有筛选出合适的白帝楼类型炼气初期正性格修士",
            "If*[&roleID>0&]#主角#随机选择了一个白帝楼类型炼气初期正性格修士[&roleID&][&roleName&]",
            "If*[&roleID>0&]#NpcDoAction*[&roleID&]#9",
            "If*[&roleID>0&]#主角#并让他在神兵阁和宝器轩挑选法宝"
        ],
        "option":[

        ]
    }
```

## VNext.DialogEnvQuery
**环境脚本**

|指令|说明|特点|范例|
|---|---|---|---|
| `string GetNPCName(int npcId)` | 根据npcId返回名字，注意大小写 | 除了一般NPC名字外，失联NPC和未绑定的工具人NPC也可以正常获取，1号是玩家名字，0号是"旁白"，如果获取失败则会返回"未知" | `"倪旭欣#我爸是[&GetNPCName(621)&]！"` 倪旭欣说我爸是倪振东 |
| `bool NearNpcContains(DialogEnvQueryContext context)` | 1.**必须**和触发器**OnNearNpc**一起使用<br>2.第一个参数为触发的npcId，可以是一个数，也可以是一个数组，只要其中任意一个在附近的人中有就能触发<br>3.第二个参数可省略，默认为100，范围从0到100，为百分比概率开启剧情事件<br>4.**使用后**会对一些环境属性赋值，可以和其他判断条件一起进行布尔运算，最终作为condition | 环境脚本中，数组的表示方式为Array(615, 614,…)注意为英文逗号<br>本环境脚本的npcId参数可兼容一个数字或者一个数组 | `"NearNpcContains(609)"` 当遇到倪旭欣时开启剧情事件 <br> `"NearNpcContains(Array(615, 614),50)"` 当遇到百里奇或者林沐心时有50%概率开启剧情事件|
| `bool RandomProbability(int roll)` | 按参数百分比概率，随机返回布尔结果 | 参数范围0~100的整数 | `RandomProbability(20)` 有20%的概率返回为真 |
| `int GetCurFubenIndex()` | 获取玩家在副本中的位置 | **仅当玩家在副本中才有效** | `"主角#我在[&GetPlaceName()&]副本第[&GetCurFubenIndex()&]位置发现了什么"`  |
| `int GetPlaceName()` | 获取玩家所在场景名称 | 相较于`GetSceneName(GetCurScene())`脚本，本环境脚本还能正确获取随机副本、玩家洞府的名称 | `"主角#我在[&GetPlaceName()&]副本第[&GetCurFubenIndex()&]位置发现了什么"` |
| `int GetShengWang(int id)` | 根据势力id返回声望 | 势力id可在配置表《str》"@势力好感度名称表"表中查询 | `"主角#我在宁州的声望是[&GetShengWang(0)&]！"` 主角自语宁州的声望 |
| `bool PlayerHasDongFu(int dongFuID)` | 返回玩家是否已有此id的洞府 |  | `"If*[&PlayerHasDongFu(1)&]#Say*主角#我已在逸风城购买洞府"` 主角自语宁州的声望 |