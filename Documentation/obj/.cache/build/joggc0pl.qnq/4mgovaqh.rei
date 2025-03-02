<!DOCTYPE html>
<!--[if IE]><![endif]-->
<html>

  <head>
    <meta charset="utf-8">
      <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
      <title>Class JObjectExtensions
 | Unity Docfx title </title>
      <meta name="viewport" content="width=device-width">
      <meta name="title" content="Class JObjectExtensions
 | Unity Docfx title ">
    
      <link rel="shortcut icon" href="../favicon.ico">
      <link rel="stylesheet" href="../styles/docfx.vendor.css">
      <link rel="stylesheet" href="../styles/docfx.css">
      <link rel="stylesheet" href="../styles/main.css">
      <meta property="docfx:navrel" content="../toc.html">
      <meta property="docfx:tocrel" content="toc.html">
    
    <meta property="docfx:rel" content="../">
    
  </head>
  <body data-spy="scroll" data-target="#affix" data-offset="120">
    <div id="wrapper">
      <header>

        <nav id="autocollapse" class="navbar navbar-inverse ng-scope" role="navigation">
          <div class="container">
            <div class="navbar-header">
              <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#navbar">
                <span class="sr-only">Toggle navigation</span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
              </button>

              <a class="navbar-brand" href="../index.html">
                <img id="logo" class="svg" src="../logo.svg" alt="">
              </a>
            </div>
            <div class="collapse navbar-collapse" id="navbar">
              <form class="navbar-form navbar-right" role="search" id="search">
                <div class="form-group">
                  <input type="text" class="form-control" id="search-query" placeholder="Search" autocomplete="off">
                </div>
              </form>
            </div>
          </div>
        </nav>

        <div class="subnav navbar navbar-default">
          <div class="container hide-when-search" id="breadcrumb">
            <ul class="breadcrumb">
              <li></li>
            </ul>
          </div>
        </div>
      </header>
      <div class="container body-content">

        <div id="search-results">
          <div class="search-list">Search Results for <span></span></div>
          <div class="sr-items">
            <p><i class="glyphicon glyphicon-refresh index-loading"></i></p>
          </div>
          <ul id="pagination" data-first="First" data-prev="Previous" data-next="Next" data-last="Last"></ul>
        </div>
      </div>
      <div role="main" class="container body-content hide-when-search">

        <div class="sidenav hide-when-search">
          <a class="btn toc-toggle collapse" data-toggle="collapse" href="#sidetoggle" aria-expanded="false" aria-controls="sidetoggle">Show / Hide Table of Contents</a>
          <div class="sidetoggle collapse" id="sidetoggle">
            <div id="sidetoc"></div>
          </div>
        </div>
        <div class="article row grid-right">
          <div class="col-md-10">
            <article class="content wrap" id="_content" data-uid="Lustie.UnityDocfx.JObjectExtensions">


  <h1 id="Lustie_UnityDocfx_JObjectExtensions" data-uid="Lustie.UnityDocfx.JObjectExtensions" class="text-break">Class JObjectExtensions
</h1>
  <div class="markdown level0 summary"></div>
  <div class="markdown level0 conceptual"></div>
  <div class="inheritance">
    <h5>Inheritance</h5>
    <div class="level0"><span class="xref">object</span></div>
    <div class="level1"><span class="xref">JObjectExtensions</span></div>
  </div>
  <div class="inheritedMembers">
    <h5>Inherited Members</h5>
    <div>
      <span class="xref">object.Equals(object)</span>
    </div>
    <div>
      <span class="xref">object.Equals(object, object)</span>
    </div>
    <div>
      <span class="xref">object.GetHashCode()</span>
    </div>
    <div>
      <span class="xref">object.GetType()</span>
    </div>
    <div>
      <span class="xref">object.MemberwiseClone()</span>
    </div>
    <div>
      <span class="xref">object.ReferenceEquals(object, object)</span>
    </div>
    <div>
      <span class="xref">object.ToString()</span>
    </div>
  </div>
  <h6><strong>Namespace</strong>: <span class="xref">Lustie</span>.<a class="xref" href="Lustie.UnityDocfx.html">UnityDocfx</a></h6>
  <h6><strong>Assembly</strong>: cs.temp.dll.dll</h6>
  <h5 id="Lustie_UnityDocfx_JObjectExtensions_syntax">Syntax</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public static class JObjectExtensions</code></pre>
  </div>
  <h3 id="methods">Methods
</h3>


  <a id="Lustie_UnityDocfx_JObjectExtensions_GetJArray_" data-uid="Lustie.UnityDocfx.JObjectExtensions.GetJArray*"></a>
  <h4 id="Lustie_UnityDocfx_JObjectExtensions_GetJArray_JObject_System_String_" data-uid="Lustie.UnityDocfx.JObjectExtensions.GetJArray(JObject,System.String)">GetJArray(JObject, string)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/Lustie.UnityDocfx.JObjectExtensions.yml" sourcestartlinenumber="2">Get property as JArray</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="declaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public static JArray GetJArray(this JObject jobject, string propertyName)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">JObject</span></td>
        <td><span class="parametername">jobject</span></td>
        <td></td>
      </tr>
      <tr>
        <td><span class="xref">string</span></td>
        <td><span class="parametername">propertyName</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">JArray</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>


  <a id="Lustie_UnityDocfx_JObjectExtensions_GetJObject_" data-uid="Lustie.UnityDocfx.JObjectExtensions.GetJObject*"></a>
  <h4 id="Lustie_UnityDocfx_JObjectExtensions_GetJObject_JObject_System_String_" data-uid="Lustie.UnityDocfx.JObjectExtensions.GetJObject(JObject,System.String)">GetJObject(JObject, string)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/Lustie.UnityDocfx.JObjectExtensions.yml" sourcestartlinenumber="2">Get property as JObject</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="declaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public static JObject GetJObject(this JObject jobject, string propertyName)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">JObject</span></td>
        <td><span class="parametername">jobject</span></td>
        <td></td>
      </tr>
      <tr>
        <td><span class="xref">string</span></td>
        <td><span class="parametername">propertyName</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">JObject</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>


  <a id="Lustie_UnityDocfx_JObjectExtensions_GetJValue_" data-uid="Lustie.UnityDocfx.JObjectExtensions.GetJValue*"></a>
  <h4 id="Lustie_UnityDocfx_JObjectExtensions_GetJValue_JObject_System_String_" data-uid="Lustie.UnityDocfx.JObjectExtensions.GetJValue(JObject,System.String)">GetJValue(JObject, string)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/Lustie.UnityDocfx.JObjectExtensions.yml" sourcestartlinenumber="2">Get property as JValue</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="declaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public static JValue GetJValue(this JObject jobject, string propertyName)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">JObject</span></td>
        <td><span class="parametername">jobject</span></td>
        <td></td>
      </tr>
      <tr>
        <td><span class="xref">string</span></td>
        <td><span class="parametername">propertyName</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">JValue</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>


  <a id="Lustie_UnityDocfx_JObjectExtensions_GetValue_" data-uid="Lustie.UnityDocfx.JObjectExtensions.GetValue*"></a>
  <h4 id="Lustie_UnityDocfx_JObjectExtensions_GetValue__1_JObject_System_String_" data-uid="Lustie.UnityDocfx.JObjectExtensions.GetValue``1(JObject,System.String)">GetValue&lt;T&gt;(JObject, string)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/Lustie.UnityDocfx.JObjectExtensions.yml" sourcestartlinenumber="2">Get property value (JToken)</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="declaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public static T GetValue&lt;T&gt;(this JObject jobject, string propertyName) where T : JToken</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">JObject</span></td>
        <td><span class="parametername">jobject</span></td>
        <td></td>
      </tr>
      <tr>
        <td><span class="xref">string</span></td>
        <td><span class="parametername">propertyName</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="returns">Returns</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">T</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
  <h5 class="typeParameters">Type Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="parametername">T</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>


  <a id="Lustie_UnityDocfx_JObjectExtensions_ReplaceValues_" data-uid="Lustie.UnityDocfx.JObjectExtensions.ReplaceValues*"></a>
  <h4 id="Lustie_UnityDocfx_JObjectExtensions_ReplaceValues_JToken_System_String_System_String_" data-uid="Lustie.UnityDocfx.JObjectExtensions.ReplaceValues(JToken,System.String,System.String)">ReplaceValues(JToken, string, string)</h4>
  <div class="markdown level1 summary"><p sourcefile="api/Lustie.UnityDocfx.JObjectExtensions.yml" sourcestartlinenumber="2">Function to recursively replace values</p>
</div>
  <div class="markdown level1 conceptual"></div>
  <h5 class="declaration">Declaration</h5>
  <div class="codewrapper">
    <pre><code class="lang-csharp hljs">public static void ReplaceValues(this JToken token, string target, string replacement)</code></pre>
  </div>
  <h5 class="parameters">Parameters</h5>
  <table class="table table-bordered table-striped table-condensed">
    <thead>
      <tr>
        <th>Type</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="xref">JToken</span></td>
        <td><span class="parametername">token</span></td>
        <td></td>
      </tr>
      <tr>
        <td><span class="xref">string</span></td>
        <td><span class="parametername">target</span></td>
        <td></td>
      </tr>
      <tr>
        <td><span class="xref">string</span></td>
        <td><span class="parametername">replacement</span></td>
        <td></td>
      </tr>
    </tbody>
  </table>
</article>
          </div>

          <div class="hidden-sm col-md-2" role="complementary">
            <div class="sideaffix">
              <div class="contribution">
                <ul class="nav">
                </ul>
              </div>
              <nav class="bs-docs-sidebar hidden-print hidden-xs hidden-sm affix" id="affix">
                <h5>In This Article</h5>
                <div></div>
              </nav>
            </div>
          </div>
        </div>
      </div>

      <footer>
        <div class="grad-bottom"></div>
        <div class="footer">
          <div class="container">
            <span class="pull-right">
              <a href="#top">Back to top</a>
            </span>
      Unity Docfx footer
      
          </div>
        </div>
      </footer>
    </div>

    <script type="text/javascript" src="../styles/docfx.vendor.js"></script>
    <script type="text/javascript" src="../styles/docfx.js"></script>
    <script type="text/javascript" src="../styles/main.js"></script>
  </body>
</html>
