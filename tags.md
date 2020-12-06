---
layout: page
title: Tags
permalink: /tags/
---

{% assign sorted_tags = site.tags | sort %}
{%- for tag in sorted_tags -%}
  {% capture tag_name %}{{ tag[0] }}{% endcapture %}
  - [`{{ tag_name }} ({{ tag[1].size }})`](/tag/{{ tag_name }})
{%- endfor -%}