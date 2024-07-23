/*
 * Copyright 2024-present Coinbase Global, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Coinbase.Core.Service
{
  public class RequestParams
  {
    private readonly string start_time;
    private readonly string end_time;
    private readonly string cursor;
    private readonly string limit;
    private readonly string sort_direction;

    public RequestParams(string start_time, string end_time, string cursor, string limit, string sort_direction)
    {
      this.start_time = start_time;
      this.end_time = end_time;
      this.cursor = cursor;
      this.limit = limit;
      this.sort_direction = sort_direction;
    }
  }
}
