using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTemplates : MonoBehaviour{
    public GameObject AStar;
    public GameObject[] bottomRooms;
    public GameObject[] topRooms;
    public GameObject[] leftRooms;
    public GameObject[] rightRooms;

    public GameObject AllAroundLevelRoom;
    //public GameObject closedRoom;
    public List<GameObject> MainEnvironmentPrefab;
    public List<GameObject> EnvironmentPrefab;
    public List<GameObject> BossEnvironmentPrefab;

    public List<GameObject> rooms;

    public float limit = 100; //X и Y максимальные и минимальныедля уровня. При части уровня = 50, а limit = 500 получится: 500 + 500 = 1000 / 50 = 20. Т.е. 20*20 = 400 комнат (В теории. т.к. не учитывает minSize и maxSize лабиринта)
    public int minMaze = 2;
    public int maxMaze = 6;
    public float waitTime;
    private bool spawnedBoss;
    public GameObject boss;
    public List<GameObject> EnemyDistributor;
    public List<GameObject> DestructibleDistributor;
    public List<GameObject> ItemDistributor;
    private int spawnedChestGuardian;
    public List<GameObject> EnemyGuardianList;
    public List<GameObject> EnemyList;
    //public List<GameObject> DestructibleList;
    [System.Serializable]
    public class ObjectsStore{
        public GameObject obj;
        public int chanceBeCreated; //Шанс создать ЭТОТ объект
    }
    public List<ObjectsStore> DestructibleList = new List<ObjectsStore>();
    public List<ObjectsStore> ItemList = new List<ObjectsStore>();

    private int spawnedChest;
    public List<GameObject> ChestList;
    public List<int> keyType;
    private int deadEndRoomsAmount = -1; //-1 т.к. в комнате босса ничего не создаю

    void Start() {
        Invoke("RoomsEnvironmentCreator", waitTime);
        //Invoke("AStarScan", waitTime + 0.1f); //Вызываю в конце RoomsObjectCreator
        //Destroy(gameObject, waitTime + 1f);
    }

    private void RoomsEnvironmentCreator() {
        for (int i = 0; i < rooms.Count; i++) { //Подсчитываем кол-во комнат с тупиком
            if (rooms[i].GetComponent<AddRoom>().deadEnd == true)
                deadEndRoomsAmount++;
            if (i != rooms.Count - 1 && i != 0) rooms[i].GetComponent<AddRoom>().CreateEnvironment(ref EnvironmentPrefab); //i != 0 && 
            else if (i == 0) rooms[i].GetComponent<AddRoom>().CreateEnvironment(ref MainEnvironmentPrefab);
            else if (i == rooms.Count - 1){
                rooms[i].GetComponent<AddRoom>().bossRoom = true;
                rooms[i].GetComponent<AddRoom>().CreateEnvironment(ref BossEnvironmentPrefab);
            }
        }
        spawnedChest = deadEndRoomsAmount / 2;
        spawnedChestGuardian = deadEndRoomsAmount / 2;

        Invoke("RoomsObjectCreator", 0.1f);
    }

    private void RoomsObjectCreator(){
        for (int i = 0; i < rooms.Count; i++){ //Не включает главную комнату (0)
            //Создаю все объекты, с которыми работаю более 1 раза за цикл
            EnemyDistributor EnDist = EnemyDistributor[i].GetComponent<EnemyDistributor>(); //Стоит добавлять в каждую комнтату иначе съезжает порядок и будет ошибка выхода за пределы списка
            DestructibleDistributor DestDist = DestructibleDistributor[i].GetComponent<DestructibleDistributor>(); //Стоит добавлять в каждую комнтату иначе съезжает порядок и будет ошибка выхода за пределы списка
            ItemDistributor ItemDist = ItemDistributor[i].GetComponent<ItemDistributor>(); //Стоит добавлять в каждую комнтату иначе съезжает порядок и будет ошибка выхода за пределы списка
            AddRoom ARoom = rooms[i].GetComponent<AddRoom>();
            if (i != rooms.Count - 1 && i != 0){
                //Создаю врагов
                int spawnEnemyAmount = Random.Range(8, 12);
                for (int k = 0; k < spawnEnemyAmount / 2 + 1; k++){ //Goblin Green
                    EnDist.SpawnEnemy(EnemyList[0], ref ARoom);
                }
                for (int k = 0; k < spawnEnemyAmount / 2 - 1; k++){ //Goblin Brown
                    EnDist.SpawnEnemy(EnemyList[1], ref ARoom);
                }
                
                //Создаю разрушаемые объекты
                DestructibleCreator(1, 5, ref DestructibleList, ref DestDist); // Случайное кол-во разрушаемых объектов от 1 до 4 (при 5 точках)
                ItemCreator(1, 4, ref ItemList, ref ItemDist); //Создаю вещи
                if (ARoom.deadEnd == true){ //rooms[i].GetComponent<AddRoom>().deadEnd
                    if (spawnedChest != 0){
                        spawnedChest--;
                    //for (int k = 0; k < 10 ; k++){
                        int rand = Random.Range(0, 100);
                        int chestNum = -1;
                        if (rand < 80){
                            keyType.Add(0); //Brown key
                            chestNum = 0;   //Brown chest
                        }
                        else if (rand < 95){
                            keyType.Add(1); //Green key
                            chestNum = 1;   //Green chest
                        }
                        else if (rand < 100){
                            keyType.Add(2); //Red key
                            chestNum = 2;   //Red chest
                        }
                        //Создаю Сундук
                        ItemDist.SpawnItem(ChestList[chestNum]);
                        //Меняю цвет комнаты
                        rooms[i].GetComponentInChildren<ItemMapOpen>().ChangeColor(new Vector3(255, 255, 0));
                    }
                    else if (spawnedChestGuardian != 0){
                        spawnedChestGuardian--;
                        int rand = Random.Range(0, keyType.Count);
                        EnDist.SpawnGuardian(ref EnemyGuardianList, 1, 0, keyType[rand]);
                        keyType.RemoveAt(rand);
                        rooms[i].GetComponentInChildren<ItemMapOpen>().ChangeColor(new Vector3(255, 193, 0));
                    }
                }
                //ARoom.EnvironmentSwitсher(false); // Отключаю окружающую среду
            }
            else if (i == 0){
                ItemCreator(1, 4, ref ItemList, ref ItemDist); //Создаю вещи
                DestructibleList[1].chanceBeCreated += 200; // Увеличиваю вероятность создания бочки с водой
                DestructibleCreator(3, 6, ref DestructibleList, ref DestDist); //Создаю разрушаемые объекты
                DestructibleList[1].chanceBeCreated -= 200; // Возвращаю вероятность создания бочки с водой
            }
            else if (i == rooms.Count - 1){ //В последней комнате появится босс
                spawnedBoss = true; //Чтобы не появился 2-й босс
                EnDist.SpawnEnemy(boss, ref ARoom);
                ItemCreator(1, 3, ref ItemList, ref ItemDist); //Создаю вещи
                DestructibleCreator(1, 5, ref DestructibleList, ref DestDist); //Создаю разрушаемые объекты
                rooms[i].GetComponentInChildren<ItemMapOpen>().ChangeColor(new Vector3(1, 195, 195));
                //Создаю разрушаемые объекты
                //int destructibleAmount = Random.Range(2, 4); // Случайное кол-во разрушаемых объектов от 2 до 3 (при 5 точках)
                //for (int k = 0; k < destructibleAmount; k++) {
                //    GameObject newObj = CalculateChanceToCreate(DestructibleList);
                //    DestDist.SpawnDestructible(newObj);
                //}
                //ARoom.EnvironmentSwitсher(false); // Отключаю окружающую среду
            }
        }

        //Вызываем AStar после всех расстановок окружающей среды
        AStarScan();
    }

    //private void CreateEnvironment(List<GameObject> EnvPrefab) {
    //    if (EnvPrefab.Count > 0){
    //        int rand = Random.Range(0, EnvPrefab.Count);
    //        rooms[i].GetComponent<AddRoom>().CreateEnvironment(EnvPrefab[rand]);
    //        EnvPrefab.RemoveAt(rand);
    //    }
    //}

    private void AStarScan(){
        AStar.GetComponent<AstarPath>().Scan();
    }

    private void DestructibleCreator(int minRand, int maxRand, ref List<ObjectsStore> DestructibleList, ref DestructibleDistributor DestDist){
        int destructibleAmount = Random.Range(minRand, maxRand); // Случайное кол-во разрушаемых объектов от 3 до 5
        for (int k = 0; k < destructibleAmount; k++){
            GameObject newObj = CalculateChanceToCreate(ref DestructibleList);
            DestDist.SpawnDestructible(newObj);
        }
    }

    private void ItemCreator(int minRand, int maxRand, ref List<ObjectsStore> ItemList, ref ItemDistributor ItemDist) {
        int itemAmount = Random.Range(minRand, maxRand); // Случайное кол-во предметов от 1 до 5 (при 10 точках)
        for (int k = 0; k < itemAmount; k++){
            GameObject newObj = CalculateChanceToCreate(ref ItemList);
            ItemDist.SpawnItem(newObj);
        }
    }

    //public int chanceCreateSomething = 100; //Шанс выпадения предмета из монстра/сундука (Чем больше, тем выше вероятность)

    public GameObject CalculateChanceToCreate(ref List<ObjectsStore> store, int chanceCreateSomething = 100){ //Передаю по ссылке т.к. ничего не меняю, а будет быстрее т.к. не будет создана копия, а будет использован этот же лист
        int calc_createChance = Random.Range(1, 101); //101 не включается
        //print("calc_dropChance = " + calc_dropChance);

        if(calc_createChance > chanceCreateSomething){ 
            Debug.Log("No Loot");
            return null;
        }
        if (calc_createChance <= chanceCreateSomething){
            int itemWeight = 0;

            for (int i = 0; i < store.Count; i++){
                itemWeight += store[i].chanceBeCreated;
            }
            Debug.Log("ItemWeight = " + itemWeight);

            int randomValue = Random.Range(0, itemWeight);

            for (int j = 0; j < store.Count; j++){
                if (randomValue <= store[j].chanceBeCreated){
                    //Instantiate(store[j].obj, transform.position, Quaternion.identity);
                    return store[j].obj;
                }
                randomValue -= store[j].chanceBeCreated;
                Debug.Log("Random Value decreased" + randomValue);
            }
        }
        return null;
    }
}
