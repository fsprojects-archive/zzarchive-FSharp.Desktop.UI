module WolframAlpha

open FSharp.Data

[<LiteralAttribute>]
let PiResponseSample = """
<queryresult success='true'
    error='false'
    numpods='5'
    datatypes=''
    timedout='Numeric,MathematicalFunctionData,Recognize'
    timedoutpods=''
    timing='0.939'
    parsetiming='0.119'
    parsetimedout='false'
    recalculate='http://www5a.wolframalpha.com/api/v2/recalc.jsp?id=MSPa217205c99h86bbih99h0000206571a048da62ag&amp;s=64'
    id='MSPa218205c99h86bbih99h000061igf05ehi2dadhc'
    host='http://www5a.wolframalpha.com'
    server='64'
    related='http://www5a.wolframalpha.com/api/v2/relatedQueries.jsp?id=MSPa219205c99h86bbih99h000031208ed793a02ebf&amp;s=64'
    version='2.6'>
 <pod title='Input'
     scanner='Identity'
     id='Input'
     position='100'
     error='false'
     numsubpods='1'>
  <subpod title=''>
   <plaintext>pi</plaintext>
  </subpod>
 </pod>
 <pod title='Decimal approximation'
     scanner='Numeric'
     id='DecimalApproximation'
     position='200'
     error='false'
     numsubpods='1'
     primary='true'>
  <subpod title=''>
   <plaintext>3.1415926535897932384626433832795028841971693993751058...</plaintext>
  </subpod>
  <states count='1'>
   <state name='More digits'
       input='DecimalApproximation__More digits' />
  </states>
 </pod>
 <pod title='Property'
     scanner='Numeric'
     id='Property'
     position='300'
     error='false'
     numsubpods='1'>
  <subpod title=''>
   <plaintext>pi is a transcendental number</plaintext>
  </subpod>
 </pod>
 <pod title='Number line'
     scanner='NumberLine'
     id='NumberLine'
     position='400'
     error='false'
     numsubpods='1'>
  <subpod title=''>
   <plaintext></plaintext>
  </subpod>
 </pod>
 <pod title='Continued fraction'
     scanner='ContinuedFraction'
     id='ContinuedFraction'
     position='500'
     error='false'
     numsubpods='1'>
  <subpod title=''>
   <plaintext>[3; 7, 15, 1, 292, 1, 1, 1, 2, 1, 3, 1, 14, 2, 1, 1, 2, 2, 2, 2, 1, 84, 2, 1, 1, 15, ...]</plaintext>
  </subpod>
  <states count='2'>
   <state name='More terms'
       input='ContinuedFraction__More terms' />
   <state name='Fraction form'
       input='ContinuedFraction__Fraction form' />
  </states>
 </pod>
 <assumptions count='1'>
  <assumption type='Clash'
      word='pi'
      template='Assuming &quot;${word}&quot; is ${desc1}. Use as ${desc2} instead'
      count='6'>
   <value name='NamedConstant'
       desc='a mathematical constant'
       input='*C.pi-_*NamedConstant-' />
   <value name='Character'
       desc='a character'
       input='*C.pi-_*Character-' />
   <value name='MathWorld'
       desc=' referring to a mathematical definition'
       input='*C.pi-_*MathWorld-' />
   <value name='MathWorldClass'
       desc='a class of mathematical terms'
       input='*C.pi-_*MathWorldClass-' />
   <value name='Movie'
       desc='a movie'
       input='*C.pi-_*Movie-' />
   <value name='Word'
       desc='a word'
       input='*C.pi-_*Word-' />
  </assumption>
 </assumptions>
</queryresult>
"""

[<LiteralAttribute>]
let ResponseSample = PiResponseSample + """
<queryresult success='false'
    error='true'
    numpods='0'
    datatypes=''
    timedout=''
    timedoutpods=''
    timing='0.024'
    parsetiming='0.'
    parsetimedout='false'
    recalculate=''
    id=''
    host='http://www3.wolframalpha.com'
    server='10'
    related=''
    version='2.6'>
 <error>
  <code>1</code>
  <msg>Invalid appid</msg>
 </error>
</queryresult>
"""
type Parser = XmlProvider<ResponseSample, true>
type Response = Parser.Queryresult

open System.Net.Http

let instance query appId = 
    async {
        use http = new HttpClient()
        let url = sprintf "http://api.wolframalpha.com/v2/query?appid=%s&input=%s&format=plaintext" appId query
        let! response = http.GetStringAsync(url) |> Async.AwaitTask
        return Parser.Parse(response)
    }
    
let airplaneMode query _ = 
    match query with
    | "pi" -> Parser.Parse(PiResponseSample)
    | unsupported -> failwithf "Unsupported test query: %s" unsupported
    |> async.Return