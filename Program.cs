using Microsoft.Web.WebView2.WinForms;

namespace SmartDesk;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public sealed class MainForm : Form
{
    private readonly WebView2 dashboard = new() { Dock = DockStyle.Fill };

    public MainForm()
    {
        Text = "SmartDesk";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 650);
        BackColor = Color.FromArgb(7, 19, 15);
        Controls.Add(dashboard);
        Load += async (_, _) =>
        {
            await dashboard.EnsureCoreWebView2Async();
            dashboard.CoreWebView2.WebMessageReceived += (_, e) =>
            {
                var url = e.TryGetWebMessageAsString();
                if (!string.IsNullOrWhiteSpace(url))
                    new BrowserForm(url).Show(this);
            };
            dashboard.NavigateToString(Html);
        };
    }

    private const string Html = """
<!doctype html><html lang="fa" dir="rtl"><head><meta charset="utf-8">
<style>
*{box-sizing:border-box}body{margin:0;height:100vh;overflow:hidden;background:radial-gradient(circle at 15% 0,#315346,#07130f 45%);color:#fff;font-family:'Vazirmatn','Segoe UI',Tahoma,sans-serif;padding:18px}
.top,.card{background:#10251fdd;border:1px solid #ffffff1c;backdrop-filter:blur(18px);border-radius:20px;box-shadow:0 18px 45px #0005}
.top{height:66px;padding:13px 20px;font-size:21px;font-weight:bold}.top small{display:block;color:#9fb5aa;font-size:11px;margin-top:5px}
.grid{height:calc(100% - 82px);margin-top:16px;display:grid;grid-template-columns:29% 42% 29%;gap:16px}.card{padding:18px;min-width:0;overflow:hidden}.links{display:grid;grid-template-columns:1fr 1fr;gap:10px}.calendar{background:#f6f8f7;color:#17231d}.calhead{display:flex;justify-content:space-between;align-items:center;margin-bottom:12px}.calgrid{display:grid;grid-template-columns:repeat(7,1fr);grid-template-rows:repeat(7,1fr);gap:6px;height:calc(100% - 50px);text-align:center}.calgrid>*{display:grid;place-items:center;border-radius:10px}.weekday{color:#718078;font-size:12px}.today{background:#10b981;color:white;font-weight:bold}.friday{color:#d84c4c}.leftcol{display:flex;flex-direction:column}.analog{width:170px;height:170px;border:5px solid #ffffff18;border-radius:50%;margin:18px auto;position:relative;background:radial-gradient(circle,#173b2d,#081711)}.hand{position:absolute;left:50%;bottom:50%;transform-origin:50% 100%;border-radius:5px}.hour{width:4px;height:45px;background:#fff}.minute{width:3px;height:62px;background:#dbe9e1}.second{width:2px;height:70px;background:#21d99a}.pin{position:absolute;left:50%;top:50%;width:10px;height:10px;border-radius:50%;background:#21d99a;transform:translate(-50%,-50%)}.weather{margin-top:10px;padding:14px;border-radius:14px;background:#ffffff0a;display:flex;justify-content:space-between}.leftcol .note{height:120px;margin-top:12px}
.link{height:84px;border:0;border-radius:15px;color:#fff;cursor:pointer;background:linear-gradient(145deg,#116147,#15946b)}.link:nth-child(even){background:linear-gradient(145deg,#29457b,#376bc5)}
input{width:100%;padding:10px;margin:6px 0;border:0;border-radius:10px}.save{width:100%;height:42px;border:0;border-radius:10px;background:#10b981;color:white;cursor:pointer;margin-top:5px}
.clock{font-size:58px;font-weight:bold;direction:ltr}.date{color:#bdd0c6;margin-top:8px}.note{width:100%;height:calc(100% - 145px);margin-top:20px;padding:15px;border:0;border-radius:15px;resize:none;font:14px 'Vazirmatn','Segoe UI',Tahoma;line-height:2}
</style></head><body>
<div class="top">SmartDesk<small>میزکار هوشمند ویندوز • WebView2</small></div>
<div class="grid">
<div class="card"><h3>دسترسی‌های کاربردی</h3><div class="links" id="links"></div><hr style="border-color:#ffffff18"><input id="title" placeholder="عنوان لینک"><input id="url" dir="ltr" placeholder="https://example.com"><button class="save" onclick="save()">ذخیره لینک</button></div>
<div class="card calendar"><div class="calhead"><button onclick="shiftMonth(-1)">ماه قبل</button><h3 id="calTitle"></h3><button onclick="shiftMonth(1)">ماه بعد</button></div><div class="calgrid" id="calendar"></div></div>
<div class="card leftcol"><div class="clock" id="clock">00:00</div><div class="date" id="date"></div><div class="analog"><div class="hand hour" id="hourHand"></div><div class="hand minute" id="minuteHand"></div><div class="hand second" id="secondHand"></div><div class="pin"></div></div><div class="weather"><span>☁️ مینودشت</span><span>هواشناسی</span></div><textarea class="note" id="note" placeholder="یادداشت سریع..."></textarea></div>
</div>
<script>
let items=JSON.parse(localStorage.smartLinks||'null')||['نهاد کتابخانه','سامانه امانت','سامانه اعضا','جستجوی کتاب','گزارش‌ها','ثبت کتاب','تنظیمات','اخبار'].map(t=>({t,u:''})),selected=0;
function render(){links.innerHTML='';items.forEach((x,i)=>{let b=document.createElement('button');b.className='link';b.textContent=x.t;b.onclick=()=>{if(x.u)chrome.webview.postMessage(x.u);else edit(i)};b.oncontextmenu=e=>{e.preventDefault();edit(i)};links.appendChild(b)})}
function edit(i){selected=i;title.value=items[i].t;url.value=items[i].u}
function save(){if(!title.value.trim())return;items[selected]={t:title.value.trim(),u:url.value.trim()};localStorage.smartLinks=JSON.stringify(items);render()}
note.value=localStorage.smartNote||'';note.oninput=()=>localStorage.smartNote=note.value;
function tick(){let d=new Date(),s=d.getSeconds(),m=d.getMinutes(),h=d.getHours();clock.textContent=d.toLocaleTimeString('fa-IR',{hour:'2-digit',minute:'2-digit'});date.textContent=new Intl.DateTimeFormat('fa-IR-u-ca-persian',{weekday:'long',year:'numeric',month:'long',day:'numeric'}).format(d);secondHand.style.transform='translateX(-50%) rotate('+(s*6)+'deg)';minuteHand.style.transform='translateX(-50%) rotate('+(m*6+s*.1)+'deg)';hourHand.style.transform='translateX(-50%) rotate('+((h%12)*30+m*.5)+'deg)'}
let monthOffset=0;function makeCal(){let base=new Date();base.setMonth(base.getMonth()+monthOffset);let parts=new Intl.DateTimeFormat('fa-IR-u-ca-persian',{year:'numeric',month:'long'}).format(base);calTitle.textContent=parts;calendar.innerHTML=['ش','ی','د','س','چ','پ','ج'].map(x=>'<b class="weekday">'+x+'</b>').join('');let today=new Intl.DateTimeFormat('fa-IR-u-ca-persian',{day:'numeric'}).format(new Date());for(let i=1;i<=35;i++){let x=document.createElement('div');if(i<=31)x.textContent=i;let pos=(i-1)%7;if(pos===6)x.classList.add('friday');if(monthOffset===0&&String(i)===today.replace(/[۰-۹]/g,d=>'۰۱۲۳۴۵۶۷۸۹'.indexOf(d)))x.classList.add('today');calendar.append(x)}}function shiftMonth(n){monthOffset+=n;makeCal()}setInterval(tick,1000);tick();makeCal();render();
</script></body></html>
""";
}

public sealed class BrowserForm : Form
{
    private readonly WebView2 web = new() { Dock = DockStyle.Fill };
    private readonly TextBox address = new() { Dock = DockStyle.Fill };
    private readonly string initialUrl;

    public BrowserForm(string url)
    {
        initialUrl = url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                     url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? url : "https://" + url;
        Text = "SmartDesk Browser";
        Width = 1200; Height = 800; StartPosition = FormStartPosition.CenterParent;

        var bar = new Panel { Dock = DockStyle.Top, Height = 42 };
        var back = new Button { Text = "←", Dock = DockStyle.Left, Width = 45 };
        var refresh = new Button { Text = "↻", Dock = DockStyle.Left, Width = 45 };
        address.Text = initialUrl;
        bar.Controls.Add(address); bar.Controls.Add(refresh); bar.Controls.Add(back);
        Controls.Add(web); Controls.Add(bar);

        Load += async (_, _) => { await web.EnsureCoreWebView2Async(); web.Source = new Uri(initialUrl); };
        back.Click += (_, _) => { if (web.CanGoBack) web.GoBack(); };
        refresh.Click += (_, _) => web.Reload();
        address.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            var u = address.Text.Trim();
            if (!u.StartsWith("http")) u = "https://" + u;
            web.Source = new Uri(u);
        };
    }
}