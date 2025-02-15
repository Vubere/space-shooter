using System;


Random random = new();
int initialTerminalHeight,initialTerminalWidth;
int initialHeight, intitalWidth;

try
{
    initialHeight = Console.WindowHeight - 2;
    intitalWidth = Console.WindowWidth - 3;
    initialTerminalHeight = Console.WindowHeight;
    initialTerminalWidth = Console.WindowWidth;
}
catch (IOException)  
{
    Console.WriteLine("Error: Console size cannot be determined.");
    return;
}

if (initialHeight < 10 || intitalWidth < 10)
{
    Console.SetCursorPosition(1, 2);
    Console.WriteLine("Terminal too small; Increase terminal size and rerun...");
    Console.ReadLine();
    Console.Clear();
    return;
}

Boundary boundary = new Boundary(initialHeight, intitalWidth, 1, 4);

boundary.height = Math.Min(boundary.height, 30);
boundary.width = Math.Min(boundary.width, 100);
Bounds playerBounds = new(boundary.playAreaStartY + 1 , boundary.height - 2, boundary.playAreaStartX+1, (boundary.width/2)-4);
Player player = new("one", playerBounds, 3, (boundary.height/2)+1);
List<Player> henchmen = [];
List<Bullet> bullets = [];
List<System.Timers.Timer> timers = [];
List<int> availableYPositions = Enumerable.Range(boundary.playAreaStartY + 1, boundary.height - boundary.playAreaStartY - 3).ToList();
List<string> henchMenDisplays = ["-[","<[]", "=o", ":["];

string GenerateStatus(string? text=null) => text!=null?text:$"health: {player.health}; Score: {player.score}";

int henchmenLimit = 4;
bool endPlay = false;
int lastStatusLength = GenerateStatus().Length;
bool gameOver = false;
Console.Title = "Space Impact";
Console.CursorVisible = false; 

int timeUntilHealthDeduction = 10;
System.Timers.Timer reduceHealthTimer = new(1000);
string warning = "Hit Enemy in 10 seconds not to lose health";

reduceHealthTimer.Elapsed += (sender, e)=> updateTimeToHealthDeduction();
void updateTimeToHealthDeduction(){
    timeUntilHealthDeduction--;
    if(timeUntilHealthDeduction==0){
        player.health-=4;
        timeUntilHealthDeduction = 10;
    }
    warning = $"Hit Enemy in {timeUntilHealthDeduction} seconds not to lose health";
};

drawGame();

while(endPlay == false)
{
    if(TerminalResized())
    {
        Console.Clear();
        reduceHealthTimer.Stop();
        reduceHealthTimer.Dispose();
        Console.Write("Width: " + Console.WindowWidth + " Height: " + Console.WindowHeight + "\n Restart! Terminal resized");
        endPlay = true;
    }
    else
    {   
        if(!gameOver){
            PaintWarning();
              PaintStatus();
              PaintBullets(ref bullets);
              PaintHenchmen(ref henchmen);
              CheckPlayerDeath(ref player);
        }
        Actions(ref player);
        Thread.Sleep(90);
    }
}
if(endPlay){
    reduceHealthTimer.Stop();
    reduceHealthTimer.Dispose();
    Console.Clear();
}

void Actions(ref Player player, int speed = 1, bool otherKeysExit = false) 
{
    int lastX = player.x;
    int lastY = player.y;
    if(Console.KeyAvailable)
    switch (Console.ReadKey(true).Key) {
    case ConsoleKey.UpArrow:
        player.y--; 
        break;
    case ConsoleKey.DownArrow: 
        player.y++; 
        break;
    case ConsoleKey.LeftArrow:  
        player.x--; 
        break;
    case ConsoleKey.RightArrow: 
        player.x++; 
        break;
    case ConsoleKey.Escape:     
        endPlay = true; 
        break;
    case ConsoleKey.Enter: {
        player.health = 100;
        player.score = 0;
        henchmen.ForEach(h=>{
            Console.SetCursorPosition(h.x, h.y);
            for (int i = 0; i < h.ToString().Length; i++)
            {
                Console.Write(" ");
            }
        });
        henchmen.Clear();
        henchmen = [];

        availableYPositions = Enumerable.Range(boundary.playAreaStartY + 1, boundary.height - boundary.playAreaStartY - 3).ToList();
        bullets.ForEach(b=>{
            Console.SetCursorPosition(b.x, b.y);
            Console.Write(" ");
        });
        bullets.Clear();
        bullets = [];
        gameOver = false;
        henchmenLimit = 4;
        timeUntilHealthDeduction = 10;
        WriteStatus(); 
        break;  
    }
    case ConsoleKey.Spacebar:
        Shoot(ref player);
        return;
    default:
        endPlay = otherKeysExit;
        break;
}
Console.SetCursorPosition(lastX, lastY);
for (int i = 0; i < player.ToString().Length; i++)
{
    Console.Write(" ");
}

player.x = Math.Clamp(player.x, player.bounds.left, player.bounds.right);
player.y = Math.Clamp(player.y, player.bounds.top, player.bounds.bottom);
Console.SetCursorPosition(player.x, player.y);
Console.Write(player.ToString());
}
void Shoot(ref Player player)
{
    Bounds bulletBounds = new(boundary.playAreaStartY + 1 , boundary.height - 2, boundary.playAreaStartX+1, boundary.width-3);
    Bullet bullet = new(player.type=="player"?player.x+3:player.x-1, player.y,bulletBounds, player.currentBullet, player.type=="player"?"right":"left");
    bullets.Add(bullet);
}
void CheckPlayerDeath(ref Player player)
{
    if(player.health<=0){
        player.health = 0;
        WriteStatus($"Game Over; Score: {player.score}; Enter To Restart;");
        gameOver = true;
    }
}
void GenerateHenchmen(int x = 0, int y = 0)
{
    int i = random.Next(0,4);
    henchmen.Add(new("henchmen", new(boundary.playAreaStartY + 1 , boundary.height - 2, (boundary.width/2)+2, boundary.width-3), x, y, henchMenDisplays[i], "henchman", i));
}
void PaintStatus()
{
    WriteStatus();
}
void PaintWarning()
{
    Console.SetCursorPosition(1,3);
    string cleaner = " ";
    for(int i = 0; i < warning.Length; i++)
        cleaner += " ";
    Console.Write(cleaner);
    warning = $"Hit Enemy in {timeUntilHealthDeduction} seconds not to lose health";
    Console.SetCursorPosition(1,3);
    Console.Write(warning);
}

void PaintBullets(ref List<Bullet> bullets)
{
    for (int i = 0; i < bullets.Count; i++)
        {
            Bullet bullet = bullets[i];
            
            int prevX = bullet.x;

            if(bullet.direction == "right")
                bullet.x += bullet.speed;
            else
                bullet.x -= bullet.speed;

            if(bullet.x > bullet.bounds.right || bullet.x < bullet.bounds.left)
            {
                Console.SetCursorPosition(prevX, bullet.y);
                Console.Write(" ");
                bullets.RemoveAt(i);
                i--;
            } 
            else
            {
                bool hit = false;
                if(bullet.direction == "right"){
                    for (int j = 0; j < henchmen.Count; j++)
                    {
                        Player henchman = henchmen[j];
                        if(henchman.x == bullet.x && henchman.y == bullet.y)
                        {
                            Console.SetCursorPosition(prevX, bullet.y);
                            Console.Write(" ");
                            Console.SetCursorPosition(henchman.x, henchman.y);
                            for (int k = 0; k < henchman.ToString().Length; k++)
                            {
                                Console.Write(" ");
                            }
                            availableYPositions.Add(henchman.y);
                            bullets.RemoveAt(i);
                            henchmen.RemoveAt(j);
                            player.score++;
                            if((player.score%10)==0&&henchmenLimit<8) {
                                henchmenLimit++;
                            }
                            timeUntilHealthDeduction = 10;
                            i--;
                            hit = true;
                            break;
                          }
                      }
                } else {
                    if(player.x == bullet.x && player.y == bullet.y)
                    {
                        Console.SetCursorPosition(prevX, bullet.y);
                        Console.Write(" ");
                        player.health-=bullet.damage;
                        if(player.health<=0){
                            hit = true;
                            break;
                        }
                        bullets.RemoveAt(i);
                        i--;
                        hit = true;
                        break;
                    }
                }
                if(hit){
                    break;
                }
                           
                Console.SetCursorPosition(prevX, bullet.y);
                Console.Write(" ");
                Console.SetCursorPosition(bullet.x, bullet.y);
                Console.Write(bullet.ToString());
                        
                bullets[i] = bullet;
            }
        }
        for (int i = bullets.Count - 1; i >= 0; i--) 
        {
            for (int j = i - 1; j >= 0; j--)  
            {
                if (bullets[i].x == bullets[j].x && bullets[i].y == bullets[j].y)
                {
                    Console.SetCursorPosition(bullets[i].x, bullets[i].y);
                    Console.Write(" ");
                    Console.SetCursorPosition(bullets[j].x, bullets[j].y);
                    Console.Write(" ");
                    bullets.RemoveAt(i);  
                    bullets.RemoveAt(j);
                    if(i>bullets.Count)
                        i = bullets.Count;
                    if(j>bullets.Count-1)
                        j = bullets.Count - 1;
                    if(i<0)
                        i = 0;
                    if(j<0)
                        j= 0;
                    break;
                }
            }
        }
}

void PaintHenchmen(ref List<Player> henchmen)
{
    if(henchmen.Count < henchmenLimit)
    {
        while (henchmen.Count < henchmenLimit && availableYPositions.Count > 0)
        {
            int x = random.Next((boundary.width / 2) + 3, boundary.width - 3);

            int index = random.Next(availableYPositions.Count);
            int y = availableYPositions[index];
            availableYPositions.RemoveAt(index);

            GenerateHenchmen(x, y);
        }

        for (int i = timers.Count -   1; i >= 0; i--) 
        { 
            timers[i].Stop();
            timers[i].Dispose();
            timers.RemoveAt(i);
        }
        for (int i = 0; i < henchmen.Count; i++)
        {
            Player henchman = henchmen[i];
            Console.SetCursorPosition(henchman.x, henchman.y);
            Console.Write(henchman.ToString());
            Shoot(ref henchman);
            System.Timers.Timer timer = new(1600);
            timer.Elapsed += (sender, e)=>Shoot(ref henchman);
            timer.Start();
            timers.Add(timer);
        }
    }
}
void WriteStatus(string? text=null) {
    Console.SetCursorPosition(1,2);
    string status = GenerateStatus();
    string cleaner = " ";
    for(int i = 0; i < lastStatusLength; i++)
        cleaner += " ";
    Console.Write(cleaner);
    Console.SetCursorPosition(1,2);
    if(text!=null){
        lastStatusLength = text.Length;
        Console.Write(text);
    }
    else
    {
        lastStatusLength = status.Length;
        Console.Write(status);
    }
}
void drawGame()
{
    Console.Clear();
    Console.SetCursorPosition(1,1);
    Console.Write($"Arrows:move;Space:shoot;Esc:quit;Enter:restart;");
    WriteStatus();
    reduceHealthTimer.Start();
 
    Console.SetCursorPosition(boundary.playAreaStartX, boundary.playAreaStartY);
    Console.Write("+");
    boundary.playAreaStartX++;
    while (boundary.playAreaStartX < boundary.width-1)
    {
        Console.SetCursorPosition(boundary.playAreaStartX, boundary.playAreaStartY);
        Console.Write("-");
        boundary.playAreaStartX++;
    }
    Console.SetCursorPosition(boundary.playAreaStartX, boundary.playAreaStartY);
    Console.Write("+");
    boundary.playAreaStartY++;
    while (boundary.playAreaStartY < boundary.height-1)
    {
        Console.SetCursorPosition(boundary.playAreaStartX, boundary.playAreaStartY);
        Console.Write("|");
        boundary.playAreaStartY++;
    }
    Console.SetCursorPosition(boundary.playAreaStartX, boundary.playAreaStartY);
    Console.Write("+");
    boundary.playAreaStartX--;
    while (boundary.playAreaStartX > 1)
    {
        Console.SetCursorPosition(boundary.playAreaStartX, boundary.playAreaStartY);
        Console.Write("-");
        boundary.playAreaStartX--;
    }
    Console.SetCursorPosition(boundary.playAreaStartX, boundary.playAreaStartY);
    Console.Write("+");
    boundary.playAreaStartY--;
    while (boundary.playAreaStartY > 4)
    {
        Console.SetCursorPosition(boundary.playAreaStartX, boundary.playAreaStartY);
        Console.Write("|");
        boundary.playAreaStartY--;
    }
    Console.SetCursorPosition(player.x, player.y);
    Console.Write(player.ToString());
}

bool TerminalResized() 
{
    if(Console.WindowHeight>boundary.height&&Console.WindowWidth>boundary.width){
        return false;
    }
    return initialTerminalHeight != Console.WindowHeight || initialTerminalWidth != Console.WindowWidth;
}
          
        

struct Bounds(int top = 0,int bottom = 0, int left = 0, int right = 0)
{
    public int top = top;
    public int bottom = bottom;
    public int left = left;
    public int right = right;
}
struct Player(string name,Bounds bounds, int x=0, int y=0, string display = "[]=", string type="player", int defaultBullet = 0)
{
    public readonly string name = name;
    public int health = 100;
    public string type = type;
    public int x = x;
    public int y = y;
    public int currentBullet = defaultBullet;
    public int score = 0;

    public Bounds bounds = bounds;

    public override readonly string ToString() => display;
};
struct Boundary (int height, int width, int playAreaStartX = 0, int playAreaStartY = 0)
{
    public int height = height;
    public int width = width;
    public int playAreaStartX = playAreaStartX;
    public int playAreaStartY = playAreaStartY;
}
struct Bullet(int x, int y,Bounds bounds, int bulletType = 0, string direction = "right" )
{
    public int x = x;
    public int y = y;
    public int speed = 1;
    public int damage = bulletType+5;
    public Bounds bounds = bounds;
    public readonly string direction = direction;
    public readonly string[] bulletTypes = ["o", "-", "<", "*"];
    public override readonly string ToString() => bulletTypes[bulletType];
}


