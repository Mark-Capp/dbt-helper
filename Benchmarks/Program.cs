// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Dbt;

BenchmarkRunner.Run<Benchmarks>();

[MemoryDiagnoser]
public class Benchmarks
{
    public string File = @"
{# outputs columns that are always used on tables with change tracking #}
{%- macro change_tracking_columns(alias_name = 'ct') -%}
{{ alias_name }}.CT_VERSION,
{{ alias_name }}.CT_DATE,
{{ alias_name }}.CT_OPERATION,
{{ alias_name }}.ID
{%- endmacro %}

{%- macro change_tracking_source(table_name, schema_name = 'dbo') -%}
{%- set source_table_name = (schema_name ~ '_' ~ table_name ~ '_CURRENT').upper() -%}
{{ source('TALENT', source_table_name) }}
{%- endmacro %}

SELECT
  {{ change_tracking_columns() }}
  , FIRST_NAME
  , LAST_NAME
  , EMAIL
  , AUTH0_ID
  , CORE_USER_ID
  , POLICY_CONSENT_GRANTED_URL
  , CREATED_AT
  , TENANT_ID
  , REQUIRES_REASONABLE_ADJUSTMENTS
  , HAS_INSIGHTS_ACCESS 
from {{ change_tracking_source('users') }} ct     
";

    private static RunCommand _command = new();

    [Benchmark]
    public string Render() => _command.Execute(File);
}