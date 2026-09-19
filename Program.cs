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
.grid{height:calc(100% - 82px);margin-top:16px;display:grid;grid-template-columns:29% 42% 29%;gap:16px}.card{padding:18px}.links{display:grid;grid-template-columns:1fr 1fr;gap:10px}
.link{height:84px;border:0;border-radius:15px;color:#fff;cursor:pointer;background:linear-gradient(145deg,#116147,#15946b)}.link:nth-child(even){background:linear-gradient(145deg,#29457b,#376bc5)}
input{width:100%;padding:10px;margin:6px 0;border:0;border-radius:10px}.save{width:100%;height:42px;border:0;border-radius:10px;background:#10b981;color:white;cursor:pointer;margin-top:5px}
.clock{font-size:58px;font-weight:bold;direction:ltr}.date{color:#bdd0c6;margin-top:8px}.note{width:100%;height:calc(100% - 145px);margin-top:20px;padding:15px;border:0;border-radius:15px;resize:none;font:14px 'Vazirmatn','Segoe UI',Tahoma;line-height:2}
</style></head><body>
<div class="top">SmartDesk<small>میزکار هوشمند ویندوز • WebView2</small></div>
<div class="grid"><div class="card"><h3>دسترسی‌های سریع</h3><div class="links" id="links"></div><hr style="border-color:#ffffff18"><input id="title" placeholder="عنوان لینک"><input id="url" dir="ltr" placeholder="https://example.com"><button class="save" onclick="save()">ذخیره لینک</button></div>
<div class="card"><div class="clock" id="clock">00:00</div><div class="date" id="date"></div><textarea class="note" id="note" placeholder="یادداشت سریع..."></textarea></div></div>
<script>
let items=JSON.parse(localStorage.smartLinks||'null')||['نهاد کتابخانه','سامانه امانت','سامانه اعضا','جستجوی کتاب','گزارش‌ها','ثبت کتاب','تنظیمات','اخبار'].map(t=>({t,u:''})),selected=0;
function render(){links.innerHTML='';items.forEach((x,i)=>{let b=document.createElement('button');b.className='link';b.textContent=x.t;b.onclick=()=>{if(x.u)chrome.webview.postMessage(x.u);else edit(i)};b.oncontextmenu=e=>{e.preventDefault();edit(i)};links.appendChild(b)})}
function edit(i){selected=i;title.value=items[i].t;url.value=items[i].u}
function save(){if(!title.value.trim())return;items[selected]={t:title.value.trim(),u:url.value.trim()};localStorage.smartLinks=JSON.stringify(items);render()}
note.value=localStorage.smartNote||'';note.oninput=()=>localStorage.smartNote=note.value;
function tick(){let d=new Date();clock.textContent=d.toLocaleTimeString('fa-IR',{hour:'2-digit',minute:'2-digit'});date.textContent=new Intl.DateTimeFormat('fa-IR-u-ca-persian',{weekday:'long',year:'numeric',month:'long',day:'numeric'}).format(d)}
function makeCal(){let h=['ش','ی','د','س','چ','پ','ج'];calendar.innerHTML=h.map(x=>'<b>'+x+'</b>').join('');for(let i=1;i<=35;i++){let d=document.createElement('div');d.textContent=i<=31?i:'';d.style.padding='10px';d.style.borderRadius='10px';if(i===new Date().getDate()){d.style.background='#10b981';d.style.color='white'}calendar.append(d)}}setInterval(tick,1000);tick();makeCal();render();
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